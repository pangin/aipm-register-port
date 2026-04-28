using AipmRegister.Core.Api;
using AipmRegister.Core.Devices;
using AipmRegister.Core.Models;
using AipmRegister.Core.Notification;
using AipmRegister.Core.Orchestration;
using AipmRegister.Core.Wifi;
using Microsoft.Extensions.Logging.Abstractions;

namespace AipmRegister.Core.Tests;

public sealed class RegistrationOrchestratorTests
{
    [Fact]
    public async Task Reports_AuthCodeInvalidOrExpired_When_GetPcKey_Returns_Null()
    {
        var notifier = new TestNotifier();
        var sut = BuildSut(
            api: new StubApi(getPcKey: _ => Task.FromResult<Account?>(null)),
            notifier: notifier);

        var result = await sut.RunAsync(SampleRequest());

        Assert.Equal(RegistrationStatus.AuthCodeInvalidOrExpired, result.Status);
        Assert.Contains(notifier.Warnings, w => w.Contains("invalid or expired", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Succeeds_When_ControlCheck_Returns_Success_On_First_Poll()
    {
        var account = new Account("u1", "k1", "37", "127");
        var sut = BuildSut(
            api: new StubApi(
                getPcKey:    _ => Task.FromResult<Account?>(account),
                controlCheck:(_, _) => Task.FromResult((ControlCheckOutcome.Success, "raw"))),
            tcp: new StubTcp(reply: "MODEL_X"));

        var result = await sut.RunAsync(SampleRequest());

        Assert.Equal(RegistrationStatus.Succeeded, result.Status);
        Assert.Equal("u1", result.UserId);
        Assert.NotNull(result.DeviceId);
        Assert.StartsWith("DAWONDNS-MODEL_X-", result.DeviceId);
    }

    [Fact]
    public async Task Reports_AlreadyRegistered_When_ControlCheck_Says_StatusError()
    {
        var account = new Account("u1", "k1", "37", "127");
        var sut = BuildSut(
            api: new StubApi(
                getPcKey:    _ => Task.FromResult<Account?>(account),
                controlCheck:(_, _) => Task.FromResult((ControlCheckOutcome.AlreadyRegistered, "STATUSERROR"))),
            tcp: new StubTcp(reply: "MODEL_X"));

        var result = await sut.RunAsync(SampleRequest());
        Assert.Equal(RegistrationStatus.AlreadyRegistered, result.Status);
    }

    [Fact]
    public async Task Reports_DeviceTcpFailed_When_Sender_Throws()
    {
        var account = new Account("u1", "k1", "37", "127");
        var sut = BuildSut(
            api: new StubApi(getPcKey: _ => Task.FromResult<Account?>(account)),
            tcp: new StubTcp(throws: new IOException("connection refused")));

        var result = await sut.RunAsync(SampleRequest());
        Assert.Equal(RegistrationStatus.DeviceTcpFailed, result.Status);
        Assert.Contains("connection refused", result.Message);
    }

    private static RegistrationRequest SampleRequest() => new(
        AuthCode8Digits:       "12345678",
        HomeSsid:              "HOME_AP",
        HomePassword:          "homepass",
        DeviceHotspotSsid:     "DAWON_IRBD_AABBCC",
        DeviceHotspotPassword: "",
        PollInterval:          TimeSpan.FromMilliseconds(1));

    private static RegistrationOrchestrator BuildSut(
        IRegisterApiClient? api = null,
        IDeviceTcpSender? tcp = null,
        IWifiAdapter? wifi = null,
        IUserNotifier? notifier = null)
    {
        return new RegistrationOrchestrator(
            api ?? new StubApi(),
            tcp ?? new StubTcp(reply: "MODEL_X"),
            wifi ?? new StubWifi(),
            notifier ?? new TestNotifier(),
            new BackendOptions(),
            NullLogger<RegistrationOrchestrator>.Instance);
    }

    private sealed class StubApi : IRegisterApiClient
    {
        private readonly Func<string, Task<Account?>> _getPcKey;
        private readonly Func<Account, string, Task<(ControlCheckOutcome, string)>> _check;

        public StubApi(
            Func<string, Task<Account?>>? getPcKey = null,
            Func<Account, string, Task<(ControlCheckOutcome, string)>>? controlCheck = null)
        {
            _getPcKey = getPcKey ?? (_ => Task.FromResult<Account?>(null));
            _check    = controlCheck ?? ((_, _) => Task.FromResult((ControlCheckOutcome.Pending, "")));
        }

        public Task<Account?> GetPcKeyAsync(string pcTempKey, CancellationToken ct = default)
            => _getPcKey(pcTempKey);

        public Task<(ControlCheckOutcome Outcome, string RawResponse)> ControlCheckAsync(
            Account account, string deviceId, CancellationToken ct = default)
            => _check(account, deviceId);
    }

    private sealed class StubTcp : IDeviceTcpSender
    {
        private readonly string _reply;
        private readonly Exception? _throws;
        public StubTcp(string reply = "", Exception? throws = null)
        {
            _reply = reply;
            _throws = throws;
        }
        public Task<string> SendSettingsAsync(string deviceHost, int port, DeviceSettings settings, CancellationToken ct = default)
            => _throws is null ? Task.FromResult(_reply) : Task.FromException<string>(_throws);
    }

    private sealed class StubWifi : IWifiAdapter
    {
        public Task<IReadOnlyList<WifiNetwork>> ScanAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<WifiNetwork>>(Array.Empty<WifiNetwork>());
        public Task ConnectAsync(string ssid, string password, WifiSecurity security, CancellationToken ct = default)
            => Task.CompletedTask;
        public Task DisconnectAndForgetAsync(string ssid, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class TestNotifier : IUserNotifier
    {
        public List<string> Infos { get; } = new();
        public List<string> Warnings { get; } = new();
        public List<string> Errors { get; } = new();
        public List<(string Stage, string Msg)> ProgressEvents { get; } = new();

        public void Info(string m)  => Infos.Add(m);
        public void Warn(string m)  => Warnings.Add(m);
        public void Error(string m, Exception? cause = null) => Errors.Add(m);
        public void Progress(string s, string m) => ProgressEvents.Add((s, m));
    }
}
