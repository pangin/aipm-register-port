using System.Net.Http;
using System.Net.Sockets;
using AipmRegister.Core.Api;
using AipmRegister.Core.Devices;
using AipmRegister.Core.Models;
using AipmRegister.Core.Network;
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

        var result = await sut.RunAsync(SampleRequest(), new StubWifi());

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
            tcp: new StubTcp(reply: "{\"respone\":\"OK\"}"));

        var result = await sut.RunAsync(SampleRequest(), new StubWifi());

        Assert.Equal(RegistrationStatus.Succeeded, result.Status);
        Assert.Equal("u1", result.UserId);
        Assert.NotNull(result.DeviceId);
        Assert.StartsWith("DAWONDNS-B530_W-", result.DeviceId);
    }

    [Fact]
    public async Task Reports_AlreadyRegistered_When_ControlCheck_Says_StatusError()
    {
        var account = new Account("u1", "k1", "37", "127");
        var sut = BuildSut(
            api: new StubApi(
                getPcKey:    _ => Task.FromResult<Account?>(account),
                controlCheck:(_, _) => Task.FromResult((ControlCheckOutcome.AlreadyRegistered, "STATUSERROR"))),
            tcp: new StubTcp(reply: "{\"respone\":\"OK\"}"));

        var result = await sut.RunAsync(SampleRequest(), new StubWifi());
        Assert.Equal(RegistrationStatus.AlreadyRegistered, result.Status);
    }

    [Fact]
    public async Task Reports_DeviceTcpFailed_When_Sender_Throws()
    {
        var account = new Account("u1", "k1", "37", "127");
        var sut = BuildSut(
            api: new StubApi(getPcKey: _ => Task.FromResult<Account?>(account)),
            tcp: new StubTcp(throws: new IOException("connection refused")));

        var result = await sut.RunAsync(SampleRequest(), new StubWifi());
        Assert.Equal(RegistrationStatus.DeviceTcpFailed, result.Status);
        Assert.Contains("connection refused", result.Message);
    }

    [Fact]
    public async Task SendDeviceSettings_Derives_Model_And_DeviceId_From_Selected_Hotspot_Not_Tcp_Reply()
    {
        var account = new Account("u1", "k1", "37", "127");
        var tcp = new StubTcp(reply: "{\"respone\":\"OK\"}");
        var sut = BuildSut(tcp: tcp);
        var picked = ProductCatalog.All.Single(p => p.Tag == "S120");

        var info = await sut.SendDeviceSettingsAsync(
            account,
            picked,
            "DWD-LS120_AABBCC",
            "HOME_AP",
            "homepass",
            "192.168.4.1",
            5000);

        Assert.Equal("B540_W", info.Model);
        Assert.Equal("DAWONDNS-B540_W-AABBCC", info.DeviceId);
        Assert.Equal("B540_W", tcp.LastSettings?.Model);
    }

    [Fact]
    public async Task HandOffToDevice_Uses_WifiGateway_As_DeviceTcpHost()
    {
        var account = new Account("u1", "k1", "37", "127");
        var tcp = new StubTcp(reply: "{\"respone\":\"OK\"}");
        var sut = BuildSut(tcp: tcp);

        await sut.HandOffToDeviceAsync(
            account,
            ProductCatalog.All[0],
            SampleRequest(),
            new StubWifi("192.168.8.1"));

        Assert.Equal("192.168.8.1", tcp.LastDeviceHost);
    }

    [Fact]
    public async Task HandOffToDevice_ReResolvesGateway_When_FirstTcpPushUsesStaleGateway()
    {
        var account = new Account("u1", "k1", "37", "127");
        var tcp = new StubTcp(
            reply: "{\"respone\":\"OK\"}",
            throwForHost: host => host == "192.168.0.1" ? new IOException("Network unreachable") : null);
        var sut = BuildSut(tcp: tcp);

        await sut.HandOffToDeviceAsync(
            account,
            ProductCatalog.All[0],
            SampleRequest(),
            new StubWifi("192.168.0.1", "192.168.8.1"));

        Assert.Equal(new[] { "192.168.0.1", "192.168.8.1" }, tcp.SentHosts);
    }

    [Fact]
    public async Task HandOffToDevice_Waits_ForInternet_After_RejoiningHomeNetwork()
    {
        var account = new Account("u1", "k1", "37", "127");
        var reachability = new RecordingReachabilityProbe();
        var wifi = new StubWifi("192.168.8.1");
        var sut = BuildSut(reachability: reachability);

        await sut.HandOffToDeviceAsync(account, ProductCatalog.All[0], SampleRequest(), wifi);

        Assert.Equal(new[] { "DWD-S120_AABBCC", "HOME_AP" }, wifi.ConnectedSsids);
        Assert.Equal("dwapi.dawonai.com", reachability.LastHost);
        Assert.Equal(18443, reachability.LastPort);
    }

    [Fact]
    public async Task RunAsync_Retries_Transient_Network_Error_During_ControlCheck()
    {
        var account = new Account("u1", "k1", "37", "127");
        var controlCheckCalls = 0;
        var sut = BuildSut(
            api: new StubApi(
                getPcKey: _ => Task.FromResult<Account?>(account),
                controlCheck: (_, _) =>
                {
                    controlCheckCalls++;
                    if (controlCheckCalls == 1)
                    {
                        var socket = new SocketException((int)SocketError.NetworkUnreachable);
                        throw new HttpRequestException("Network unreachable", socket);
                    }

                    return Task.FromResult((ControlCheckOutcome.Success, "raw"));
                }),
            reachability: new NoopReachabilityProbe());

        var result = await sut.RunAsync(
            SampleRequest() with { PollInterval = TimeSpan.FromMilliseconds(1) },
            new StubWifi("192.168.8.1"));

        Assert.Equal(RegistrationStatus.Succeeded, result.Status);
        Assert.Equal(2, controlCheckCalls);
    }

    [Fact]
    public async Task PollRegistration_DoesNotTerminate_OnFirstNotRegistered()
    {
        // frmMain.cs:2366 only declared NOTREGISTERED terminal after the
        // 5s polling timer had ticked > 10 times (≈55s window). The
        // pre-v1.5.2 RegisteringViewModel returned on the very first
        // NOTREGISTERED tick — this test pins the orchestrator's correct
        // "keep polling" behavior so the macOS regression cannot resurface.
        var account = new Account("u1", "k1", "37", "127");
        var sut = BuildSut(api: new StubApi(
            controlCheck: (_, _) =>
                Task.FromResult((ControlCheckOutcome.NotRegistered, "NOTREGISTERED"))));

        var ticks = new List<ControlCheckTick>();
        await foreach (var tick in sut.PollRegistrationAsync(
                           account, "id", maxAttempts: 5, TimeSpan.Zero))
        {
            ticks.Add(tick);
        }

        Assert.Equal(5, ticks.Count);
        Assert.All(ticks, t => Assert.Equal(ControlCheckOutcome.NotRegistered, t.Outcome));
    }

    [Fact]
    public async Task PollRegistration_TerminatesAfter_NotRegistered_Crosses_Threshold()
    {
        // The threshold is "++notRegistered > 25" so the 26th NOTREGISTERED
        // tick is the last one yielded. ≈50s of polling at 2s pollInterval —
        // matches frmMain.cs:2366 + m_6.Interval=5000, m_8>10.
        var account = new Account("u1", "k1", "37", "127");
        var sut = BuildSut(api: new StubApi(
            controlCheck: (_, _) =>
                Task.FromResult((ControlCheckOutcome.NotRegistered, "NOTREGISTERED"))));

        var ticks = new List<ControlCheckTick>();
        await foreach (var tick in sut.PollRegistrationAsync(
                           account, "id", maxAttempts: 100, TimeSpan.Zero))
        {
            ticks.Add(tick);
        }

        Assert.Equal(26, ticks.Count);
    }

    private static RegistrationRequest SampleRequest() => new(
        AuthCode8Digits:       "12345678",
        HomeSsid:              "HOME_AP",
        HomePassword:          "homepass",
        DeviceHotspotSsid:     "DWD-S120_AABBCC",
        DeviceHotspotPassword: "",
        PollInterval:          TimeSpan.FromMilliseconds(1));

    private static RegistrationOrchestrator BuildSut(
        IRegisterApiClient? api = null,
        IDeviceTcpSender? tcp = null,
        IUserNotifier? notifier = null,
        IInternetReachabilityProbe? reachability = null)
    {
        return new RegistrationOrchestrator(
            api ?? new StubApi(),
            tcp ?? new StubTcp(reply: "MODEL_X"),
            notifier ?? new TestNotifier(),
            new BackendOptions(),
            reachability ?? new NoopReachabilityProbe(),
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
        private readonly Func<string, Exception?>? _throwForHost;
        public string? LastDeviceHost { get; private set; }
        public DeviceSettings? LastSettings { get; private set; }
        public List<string> SentHosts { get; } = new();
        public StubTcp(string reply = "", Exception? throws = null, Func<string, Exception?>? throwForHost = null)
        {
            _reply = reply;
            _throws = throws;
            _throwForHost = throwForHost;
        }
        public Task<string> SendSettingsAsync(
            string deviceHost,
            int port,
            DeviceSettings settings,
            CancellationToken ct = default)
        {
            LastDeviceHost = deviceHost;
            LastSettings = settings;
            SentHosts.Add(deviceHost);
            var error = _throwForHost?.Invoke(deviceHost) ?? _throws;
            return error is null ? Task.FromResult(_reply) : Task.FromException<string>(error);
        }
    }

    private sealed class StubWifi : IWifiAdapter, IWifiGatewayProvider
    {
        private readonly Queue<string?> _gateways;
        public List<string> ConnectedSsids { get; } = new();

        public StubWifi(params string?[] gateways) => _gateways = new Queue<string?>(gateways);

        public Task<IReadOnlyList<WifiNetwork>> ScanAsync(CancellationToken ct = default)
            => Task.FromResult<IReadOnlyList<WifiNetwork>>(Array.Empty<WifiNetwork>());
        public Task ConnectAsync(string ssid, string password, WifiSecurity security, CancellationToken ct = default)
        {
            ConnectedSsids.Add(ssid);
            return Task.CompletedTask;
        }
        public Task DisconnectAndForgetAsync(string ssid, CancellationToken ct = default)
            => Task.CompletedTask;
        public Task<string?> GetGatewayAsync(TimeSpan timeout, CancellationToken ct = default)
        {
            if (_gateways.Count == 0) return Task.FromResult<string?>(null);
            if (_gateways.Count == 1) return Task.FromResult(_gateways.Peek());
            return Task.FromResult(_gateways.Dequeue());
        }
    }

    private sealed class NoopReachabilityProbe : IInternetReachabilityProbe
    {
        public Task WaitUntilReachableAsync(string host, int port, TimeSpan timeout, CancellationToken ct = default)
            => Task.CompletedTask;
    }

    private sealed class RecordingReachabilityProbe : IInternetReachabilityProbe
    {
        public string? LastHost { get; private set; }
        public int LastPort { get; private set; }

        public Task WaitUntilReachableAsync(string host, int port, TimeSpan timeout, CancellationToken ct = default)
        {
            LastHost = host;
            LastPort = port;
            return Task.CompletedTask;
        }
    }

    private sealed class TestNotifier : IUserNotifier
    {
        public List<string> Infos { get; } = new();
        public List<string> Warnings { get; } = new();
        public List<string> Errors { get; } = new();
        public List<(RegistrationStage Stage, string Msg)> ProgressEvents { get; } = new();

        public void Info(string m)  => Infos.Add(m);
        public void Warn(string m)  => Warnings.Add(m);
        public void Error(string m, Exception? cause = null) => Errors.Add(m);
        public void Progress(RegistrationStage s, string m) => ProgressEvents.Add((s, m));
    }
}
