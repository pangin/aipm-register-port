using System.Runtime.CompilerServices;
using System.Net.Http;
using System.Net.Sockets;
using AipmRegister.Core.Api;
using AipmRegister.Core.Devices;
using AipmRegister.Core.Models;
using AipmRegister.Core.Network;
using AipmRegister.Core.Notification;
using AipmRegister.Core.Wifi;
using Microsoft.Extensions.Logging;

namespace AipmRegister.Core.Orchestration;

public sealed class RegistrationOrchestrator : IRegistrationOrchestrator
{
    private readonly IRegisterApiClient _api;
    private readonly IDeviceTcpSender _tcp;
    private readonly IUserNotifier _notifier;
    private readonly BackendOptions _backend;
    private readonly IInternetReachabilityProbe _reachability;
    private readonly ILogger<RegistrationOrchestrator> _logger;

    public RegistrationOrchestrator(
        IRegisterApiClient api,
        IDeviceTcpSender tcp,
        IUserNotifier notifier,
        BackendOptions backend,
        IInternetReachabilityProbe reachability,
        ILogger<RegistrationOrchestrator> logger)
    {
        _api = api;
        _tcp = tcp;
        _notifier = notifier;
        _backend = backend;
        _reachability = reachability;
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
        var mac = HotspotSsidParser.ExtractMac(deviceHotspotSsid);
        var modelCode = ProductCatalog.ResolveModelCode(deviceHotspotSsid, picked);
        var settings = BuildDeviceSettings(account, homeSsid, homePassword, mac, modelCode);
        var reply = await _tcp.SendSettingsAsync(deviceTcpHost, deviceTcpPort, settings, ct);
        if (!string.IsNullOrWhiteSpace(reply))
        {
            _logger.LogDebug("Device TCP reply: {Reply}", reply.Trim());
        }

        var deviceId = $"{_backend.Company}-{modelCode}-{mac}";
        _logger.LogInformation("Assembled deviceId={DeviceId} (model={Model}, mac={Mac})", deviceId, modelCode, mac);
        return new DeviceModelInfo(mac, modelCode, deviceId);
    }

    public async Task<DeviceModelInfo> HandOffToDeviceAsync(
        Account account,
        ProductDefinition picked,
        RegistrationRequest request,
        IWifiAdapter wifi,
        CancellationToken ct = default)
    {
        _notifier.Progress(RegistrationStage.Wifi, $"Connecting to device hotspot \"{request.DeviceHotspotSsid}\"...");
        await wifi.ConnectAsync(
            request.DeviceHotspotSsid,
            request.DeviceHotspotPassword,
            WifiSecurity.Open,
            ct);

        var info = await SendDeviceSettingsWithGatewayRefreshAsync(account, picked, request, wifi, ct);

        await wifi.DisconnectAndForgetAsync(request.DeviceHotspotSsid, ct);
        _notifier.Progress(RegistrationStage.Wifi, $"Rejoining home network \"{request.HomeSsid}\"...");
        await wifi.ConnectAsync(request.HomeSsid, request.HomePassword, request.HomeSecurity, ct);
        _notifier.Progress(RegistrationStage.Wifi, $"Waiting for internet on \"{request.HomeSsid}\"...");
        await _reachability.WaitUntilReachableAsync(_backend.ApiHost, _backend.ApiPort, TimeSpan.FromSeconds(30), ct);

        return info;
    }

    private async Task<DeviceModelInfo> SendDeviceSettingsWithGatewayRefreshAsync(
        Account account,
        ProductDefinition picked,
        RegistrationRequest request,
        IWifiAdapter wifi,
        CancellationToken ct)
    {
        var deviceTcpHost = await ResolveDeviceTcpHostAsync(wifi, request, ct);

        try
        {
            return await PushDeviceSettingsAsync(account, picked, request, deviceTcpHost, ct);
        }
        catch (Exception ex) when (IsTransientNetworkError(ex))
        {
            var refreshedHost = await ResolveDeviceTcpHostAsync(wifi, request, ct);
            if (string.Equals(refreshedHost, deviceTcpHost, StringComparison.Ordinal))
            {
                throw;
            }

            _logger.LogWarning(
                ex,
                "Device TCP push failed at {OldHost}; retrying with refreshed gateway {NewHost}.",
                deviceTcpHost,
                refreshedHost);

            return await PushDeviceSettingsAsync(account, picked, request, refreshedHost, ct);
        }
    }

    private Task<DeviceModelInfo> PushDeviceSettingsAsync(
        Account account,
        ProductDefinition picked,
        RegistrationRequest request,
        string deviceTcpHost,
        CancellationToken ct)
    {
        _notifier.Progress(RegistrationStage.Device, $"Pushing settings to device at {deviceTcpHost}:{request.DeviceTcpPort}...");
        return SendDeviceSettingsAsync(
            account, picked,
            request.DeviceHotspotSsid,
            request.HomeSsid, request.HomePassword,
            deviceTcpHost, request.DeviceTcpPort,
            ct);
    }

    private async Task<string> ResolveDeviceTcpHostAsync(
        IWifiAdapter wifi,
        RegistrationRequest request,
        CancellationToken ct)
    {
        if (wifi is not IWifiGatewayProvider gatewayProvider) return request.DeviceTcpHost;

        var gateway = await gatewayProvider.GetGatewayAsync(TimeSpan.FromSeconds(20), ct);
        if (string.IsNullOrWhiteSpace(gateway)) return request.DeviceTcpHost;

        if (!string.Equals(gateway, request.DeviceTcpHost, StringComparison.Ordinal))
        {
            _logger.LogInformation(
                "Using Wi-Fi gateway {Gateway} as device TCP host instead of configured host {ConfiguredHost}.",
                gateway,
                request.DeviceTcpHost);
        }

        return gateway;
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
            ControlCheckOutcome outcome;
            string raw;

            try
            {
                (outcome, raw) = await _api.ControlCheckAsync(account, deviceId, ct);
            }
            catch (Exception ex) when (IsTransientNetworkError(ex))
            {
                _logger.LogWarning(ex, "control/check attempt {Attempt}/{Max} failed while the network is recovering.", attempt, maxAttempts);
                outcome = ControlCheckOutcome.Pending;
                raw = ex.Message;
            }

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

    public async Task<RegistrationResult> RunAsync(RegistrationRequest request, IWifiAdapter wifi, CancellationToken ct = default)
    {
        var pollInterval = request.PollInterval ?? TimeSpan.FromSeconds(2);

        _notifier.Progress(RegistrationStage.Auth, "Exchanging 8-digit code for account info...");
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

        var picked = HotspotSsidParser.ResolveProduct(request.DeviceHotspotSsid);
        DeviceModelInfo info;
        try
        {
            info = await HandOffToDeviceAsync(account, picked, request, wifi, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _notifier.Error("Wi-Fi / TCP hand-off to device failed.", ex);
            return new RegistrationResult(RegistrationStatus.DeviceTcpFailed, account.UserId, null, ex.Message);
        }

        try
        {
            _notifier.Progress(RegistrationStage.Poll, "Waiting for cloud to register the device...");
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

    private static bool IsTransientNetworkError(Exception ex)
    {
        if (ex is HttpRequestException or IOException) return true;

        for (var current = ex; current is not null; current = current.InnerException)
        {
            if (current is SocketException socketException && IsTransientSocketError(socketException.SocketErrorCode))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsTransientSocketError(SocketError error)
        => error is SocketError.NetworkUnreachable
            or SocketError.HostUnreachable
            or SocketError.HostNotFound
            or SocketError.TryAgain
            or SocketError.TimedOut
            or SocketError.ConnectionRefused;
}
