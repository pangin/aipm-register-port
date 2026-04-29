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
    private readonly IUserNotifier _notifier;
    private readonly BackendOptions _backend;
    private readonly ILogger<RegistrationOrchestrator> _logger;

    public RegistrationOrchestrator(
        IRegisterApiClient api,
        IDeviceTcpSender tcp,
        IUserNotifier notifier,
        BackendOptions backend,
        ILogger<RegistrationOrchestrator> logger)
    {
        _api = api;
        _tcp = tcp;
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
        var mac = HotspotSsidParser.ExtractMac(deviceHotspotSsid);
        var settings = BuildDeviceSettings(account, homeSsid, homePassword, mac, picked.ModelCode);
        var reply = await _tcp.SendSettingsAsync(deviceTcpHost, deviceTcpPort, settings, ct);

        // The device replies with its own SSID-style identifier; resolve via
        // the catalog so our model code matches what the cloud assigns.
        var trimmedReply = reply?.Trim().Trim('"') ?? string.Empty;
        var modelCode = string.IsNullOrEmpty(trimmedReply)
            ? picked.ModelCode
            : ProductCatalog.ResolveModelCode(trimmedReply, picked);
        if (string.IsNullOrEmpty(modelCode))
        {
            // Catalog had no matching SKU — preserve v0.3 behavior of using
            // whatever the device reported back as the model.
            modelCode = string.IsNullOrEmpty(trimmedReply) ? picked.ModelCode : trimmedReply;
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

        _notifier.Progress(RegistrationStage.Device, $"Pushing settings to device at {request.DeviceTcpHost}:{request.DeviceTcpPort}...");
        var info = await SendDeviceSettingsAsync(
            account, picked,
            request.DeviceHotspotSsid,
            request.HomeSsid, request.HomePassword,
            request.DeviceTcpHost, request.DeviceTcpPort,
            ct);

        await wifi.DisconnectAndForgetAsync(request.DeviceHotspotSsid, ct);
        _notifier.Progress(RegistrationStage.Wifi, $"Rejoining home network \"{request.HomeSsid}\"...");
        await wifi.ConnectAsync(request.HomeSsid, request.HomePassword, WifiSecurity.Wpa2Personal, ct);

        return info;
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
}
