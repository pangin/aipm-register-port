using System.Runtime.CompilerServices;
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

    // ----- Step hooks (used by the wizard GUI) ------------------------------

    public Task<Account?> ExchangeAuthCodeAsync(string authCode8Digits, CancellationToken ct = default)
        => _api.GetPcKeyAsync(authCode8Digits, ct);

    public async Task<DeviceModelInfo> SendDeviceSettingsAsync(
        Account account,
        ProductDefinition picked,
        string deviceHotspotSsid,
        string homeSsid,
        string homePassword,
        string deviceTcpHost,
        int deviceTcpPort,
        CancellationToken ct = default)
    {
        var mac = ResolveDeviceMac(deviceHotspotSsid);
        var settings = BuildDeviceSettings(account, homeSsid, homePassword, mac, picked.ModelCode);
        var reply = await _tcp.SendSettingsAsync(deviceTcpHost, deviceTcpPort, settings, ct);

        // The device replies with its own SSID-style identifier; resolve via
        // the catalog so our model code matches what the cloud assigns.
        var modelCode = string.IsNullOrWhiteSpace(reply)
            ? picked.ModelCode
            : ProductCatalog.ResolveModelCode(reply.Trim().Trim('"'), picked);
        if (string.IsNullOrEmpty(modelCode)) modelCode = picked.ModelCode;

        var deviceId = $"{_backend.Company}-{modelCode}-{mac}";
        _logger.LogInformation("Assembled deviceId={DeviceId} (model={Model}, mac={Mac})", deviceId, modelCode, mac);
        return new DeviceModelInfo(mac, modelCode, deviceId);
    }

    public async IAsyncEnumerable<ControlCheckTick> PollRegistrationAsync(
        Account account,
        string deviceId,
        int maxAttempts,
        TimeSpan pollInterval,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        int notRegistered = 0;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            ct.ThrowIfCancellationRequested();
            var (outcome, raw) = await _api.ControlCheckAsync(account, deviceId, ct);
            _logger.LogDebug("Attempt {Attempt}/{Max}: {Outcome}", attempt, maxAttempts, outcome);

            yield return new ControlCheckTick(attempt, maxAttempts, outcome, raw);

            switch (outcome)
            {
                case ControlCheckOutcome.Success:
                case ControlCheckOutcome.AlreadyRegistered:
                case ControlCheckOutcome.AuthCodeExpired:
                    yield break;

                case ControlCheckOutcome.NotRegisteredExceededAttempts:
                    if (++notRegistered > 10) yield break;
                    break;
            }

            if (attempt < maxAttempts)
            {
                await Task.Delay(pollInterval, ct);
            }
        }
    }

    // ----- All-in-one (CLI) -------------------------------------------------

    public async Task<RegistrationResult> RunAsync(RegistrationRequest request, CancellationToken ct = default)
    {
        var pollInterval = request.PollInterval ?? TimeSpan.FromSeconds(2);

        _notifier.Progress("auth", "Exchanging 8-digit code for account info...");
        Account? account;
        try
        {
            account = await ExchangeAuthCodeAsync(request.AuthCode8Digits, ct);
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
            // Use the catalog if a tag was provided, otherwise fall back to the
            // legacy heuristic (the CLI still accepts free-form hotspot SSIDs).
            var picked = ResolveProductFromHotspotSsid(request.DeviceHotspotSsid);
            DeviceModelInfo info;
            try
            {
                _notifier.Progress("device", $"Pushing settings to device at {request.DeviceTcpHost}:{request.DeviceTcpPort}...");
                info = await SendDeviceSettingsAsync(
                    account, picked,
                    request.DeviceHotspotSsid,
                    request.HomeSsid, request.HomePassword,
                    request.DeviceTcpHost, request.DeviceTcpPort,
                    ct);
            }
            catch (Exception ex)
            {
                _notifier.Error("TCP push to device failed.", ex);
                return new RegistrationResult(RegistrationStatus.DeviceTcpFailed, account.UserId, null, ex.Message);
            }

            await _wifi.DisconnectAndForgetAsync(request.DeviceHotspotSsid, ct);
            _notifier.Progress("wifi", $"Rejoining home network \"{request.HomeSsid}\"...");
            await _wifi.ConnectAsync(request.HomeSsid, request.HomePassword, WifiSecurity.Wpa2Personal, ct);

            _notifier.Progress("poll", "Waiting for cloud to register the device...");
            ControlCheckTick? terminal = null;
            await foreach (var tick in PollRegistrationAsync(account, info.DeviceId,
                                       request.MaxControlCheckAttempts, pollInterval, ct))
            {
                terminal = tick;
                if (tick.Outcome is ControlCheckOutcome.Success
                                  or ControlCheckOutcome.AlreadyRegistered
                                  or ControlCheckOutcome.AuthCodeExpired)
                {
                    break;
                }
            }

            return terminal?.Outcome switch
            {
                ControlCheckOutcome.Success                      => new RegistrationResult(RegistrationStatus.Succeeded,                 account.UserId, info.DeviceId, null),
                ControlCheckOutcome.AlreadyRegistered            => new RegistrationResult(RegistrationStatus.AlreadyRegistered,         account.UserId, info.DeviceId, "STATUSERROR"),
                ControlCheckOutcome.AuthCodeExpired              => new RegistrationResult(RegistrationStatus.AuthCodeInvalidOrExpired,  account.UserId, info.DeviceId, "TIMEFAILED"),
                ControlCheckOutcome.NotRegisteredExceededAttempts=> new RegistrationResult(RegistrationStatus.DeviceNotResponding,      account.UserId, info.DeviceId, "Device did not register after repeated NOTREGISTERED responses."),
                _                                                => new RegistrationResult(RegistrationStatus.DeviceNotResponding,      account.UserId, info.DeviceId, $"Timed out after {request.MaxControlCheckAttempts} polls."),
            };
        }
        catch (OperationCanceledException)
        {
            return new RegistrationResult(RegistrationStatus.Cancelled, account.UserId, null, "Cancelled by user.");
        }
    }

    // ----- Helpers ----------------------------------------------------------

    private DeviceSettings BuildDeviceSettings(Account account, string homeSsid, string homePassword, string mac, string model) => new(
        Mac:               mac,
        ApiServerAddress:  _backend.ApiHost,
        ApiServerPort:     _backend.ApiPort.ToString(),
        MqttServerAddress: _backend.MqttHost,
        MqttServerPort:    _backend.MqttPort.ToString(),
        SslSupport:        _backend.SslSupport,
        HomeSsid:          homeSsid,
        HomePassword:      homePassword,
        UserId:            account.UserId,
        Company:           _backend.Company,
        Model:             model,
        Latitude:          account.Latitude,
        Longitude:         account.Longitude,
        Topic:             _backend.Topic);

    /// Walk the catalog and pick whatever product owns the given hotspot SSID.
    /// Falls back to a synthetic ProductDefinition built from the SSID prefix
    /// so the CLI can still target unknown SKUs.
    private static ProductDefinition ResolveProductFromHotspotSsid(string ssid)
    {
        foreach (var p in ProductCatalog.All)
        {
            if (p.IsHotspotOf(ssid)) return p;
        }
        var token = ssid.Split('_')[0];
        return new ProductDefinition(
            Tag: token.StartsWith("DWD-", StringComparison.Ordinal) ? token[4..] : token,
            DisplayKey: "Product.Unknown.Name",
            IconKey: "Icon.SmartPlug",
            PrimaryPrefix: token,
            SecondaryPrefix: string.Empty,
            ModelCode: "UNKNOWN");
    }

    /// Recovered from frmMain (lines 1862~1864): the device's own hotspot
    /// SSID encodes the MAC suffix.
    private static string ResolveDeviceMac(string hotspotSsid)
    {
        var parts = hotspotSsid.Split('_', '-');
        return parts.Length > 0 ? parts[^1] : hotspotSsid;
    }
}
