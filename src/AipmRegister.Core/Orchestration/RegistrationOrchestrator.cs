using AipmRegister.Core.Api;
using AipmRegister.Core.Devices;
using AipmRegister.Core.Models;
using AipmRegister.Core.Notification;
using AipmRegister.Core.Wifi;
using Microsoft.Extensions.Logging;

namespace AipmRegister.Core.Orchestration;

public sealed class RegistrationOrchestrator : IRegistrationOrchestrator
{
    private readonly IRegisterApiClient _api;
    private readonly IDeviceTcpSender _tcp;
    private readonly IWifiAdapter _wifi;
    private readonly IUserNotifier _notifier;
    private readonly BackendOptions _backend;
    private readonly ILogger<RegistrationOrchestrator> _logger;

    public RegistrationOrchestrator(
        IRegisterApiClient api,
        IDeviceTcpSender tcp,
        IWifiAdapter wifi,
        IUserNotifier notifier,
        BackendOptions backend,
        ILogger<RegistrationOrchestrator> logger)
    {
        _api = api;
        _tcp = tcp;
        _wifi = wifi;
        _notifier = notifier;
        _backend = backend;
        _logger = logger;
    }

    public async Task<RegistrationResult> RunAsync(RegistrationRequest request, CancellationToken ct = default)
    {
        var pollInterval = request.PollInterval ?? TimeSpan.FromSeconds(2);

        // Stage 1 — exchange auth code for account
        _notifier.Progress("auth", "Exchanging 8-digit code for account info...");
        Account? account;
        try
        {
            account = await _api.GetPcKeyAsync(request.AuthCode8Digits, ct);
        }
        catch (ArgumentException ex)
        {
            _notifier.Error(ex.Message, ex);
            return new RegistrationResult(RegistrationStatus.AuthCodeInvalidOrExpired, null, null, ex.Message);
        }

        if (account is null)
        {
            const string msg = "Auth code is invalid or expired. Re-issue from the mobile app.";
            _notifier.Warn(msg);
            return new RegistrationResult(RegistrationStatus.AuthCodeInvalidOrExpired, null, null, msg);
        }
        _logger.LogInformation("Account ok: user_id={UserId}", account.UserId);

        // Stage 2 — connect to device hotspot
        _notifier.Progress("wifi", $"Connecting to device hotspot \"{request.DeviceHotspotSsid}\"...");
        try
        {
            await _wifi.ConnectAsync(
                request.DeviceHotspotSsid,
                request.DeviceHotspotPassword,
                WifiSecurity.Open,
                ct);
        }
        catch (Exception ex)
        {
            _notifier.Error("Failed to join device hotspot.", ex);
            return new RegistrationResult(RegistrationStatus.DeviceNotResponding, account.UserId, null, ex.Message);
        }

        try
        {
            // Stage 3 — push settings over TCP, derive device_id
            _notifier.Progress("device", $"Pushing settings to device at {request.DeviceTcpHost}:{request.DeviceTcpPort}...");
            string mac = ResolveDeviceMac(request.DeviceHotspotSsid);
            string model;
            try
            {
                var settings = BuildDeviceSettings(account, request, mac);
                var reply = await _tcp.SendSettingsAsync(
                    request.DeviceTcpHost, request.DeviceTcpPort, settings, ct);
                model = ExtractModelFromReply(reply);
            }
            catch (Exception ex)
            {
                _notifier.Error("TCP push to device failed.", ex);
                return new RegistrationResult(RegistrationStatus.DeviceTcpFailed, account.UserId, null, ex.Message);
            }

            string deviceId = $"{_backend.Company}-{model}-{mac}";
            _logger.LogInformation("Device id assembled: {DeviceId}", deviceId);

            // Stage 4 — leave hotspot, rejoin home network so we can poll cloud
            await _wifi.DisconnectAndForgetAsync(request.DeviceHotspotSsid, ct);
            _notifier.Progress("wifi", $"Rejoining home network \"{request.HomeSsid}\"...");
            await _wifi.ConnectAsync(request.HomeSsid, request.HomePassword, WifiSecurity.Wpa2Personal, ct);

            // Stage 5 — poll registration status
            _notifier.Progress("poll", "Waiting for cloud to register the device...");
            int notRegistered = 0;
            for (int attempt = 1; attempt <= request.MaxControlCheckAttempts; attempt++)
            {
                ct.ThrowIfCancellationRequested();
                var (outcome, raw) = await _api.ControlCheckAsync(account, deviceId, ct);
                _logger.LogDebug("Attempt {Attempt}: {Outcome} :: {Raw}", attempt, outcome, raw);

                switch (outcome)
                {
                    case ControlCheckOutcome.Success:
                        _notifier.Info("Registration succeeded.");
                        return new RegistrationResult(RegistrationStatus.Succeeded, account.UserId, deviceId, null);

                    case ControlCheckOutcome.AlreadyRegistered:
                        _notifier.Warn("Device already registered to another account.");
                        return new RegistrationResult(RegistrationStatus.AlreadyRegistered, account.UserId, deviceId, "STATUSERROR");

                    case ControlCheckOutcome.AuthCodeExpired:
                        return new RegistrationResult(RegistrationStatus.AuthCodeInvalidOrExpired, account.UserId, deviceId, "TIMEFAILED");

                    case ControlCheckOutcome.NotRegisteredExceededAttempts:
                        if (++notRegistered > 10)
                        {
                            const string msg = "Device did not register after 10 NOTREGISTERED responses. Reset the device and retry.";
                            _notifier.Warn(msg);
                            return new RegistrationResult(RegistrationStatus.DeviceNotResponding, account.UserId, deviceId, msg);
                        }
                        break;

                    case ControlCheckOutcome.UnknownError:
                    case ControlCheckOutcome.Pending:
                    default:
                        break;
                }

                await Task.Delay(pollInterval, ct);
            }

            return new RegistrationResult(
                RegistrationStatus.DeviceNotResponding,
                account.UserId,
                deviceId,
                $"Timed out after {request.MaxControlCheckAttempts} polls.");
        }
        catch (OperationCanceledException)
        {
            return new RegistrationResult(RegistrationStatus.Cancelled, account.UserId, null, "Cancelled by user.");
        }
    }

    private DeviceSettings BuildDeviceSettings(Account account, RegistrationRequest req, string mac) => new(
        Mac:               mac,
        ApiServerAddress:  _backend.ApiHost,
        ApiServerPort:     _backend.ApiPort.ToString(),
        MqttServerAddress: _backend.MqttHost,
        MqttServerPort:    _backend.MqttPort.ToString(),
        SslSupport:        _backend.SslSupport,
        HomeSsid:          req.HomeSsid,
        HomePassword:      req.HomePassword,
        UserId:             account.UserId,
        Company:           _backend.Company,
        Model:             string.Empty, // placeholder; device replies with model
        Latitude:          account.Latitude,
        Longitude:         account.Longitude,
        Topic:             _backend.Topic);

    /// Heuristic recovered from frmMain (lines 1862~1864): the device's own
    /// hotspot SSID encodes the MAC suffix, so we use that as the canonical
    /// MAC. Real implementations will replace this with the value the device
    /// reports during the TCP exchange.
    private static string ResolveDeviceMac(string hotspotSsid)
    {
        var parts = hotspotSsid.Split('_', '-');
        return parts.Length > 0 ? parts[^1] : hotspotSsid;
    }

    private static string ExtractModelFromReply(string reply)
    {
        if (string.IsNullOrWhiteSpace(reply)) return "UNKNOWN";
        var trimmed = reply.Trim().Trim('"');
        return trimmed.Length == 0 ? "UNKNOWN" : trimmed;
    }
}
