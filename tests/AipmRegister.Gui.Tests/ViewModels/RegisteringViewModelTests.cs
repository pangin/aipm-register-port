using System.Collections.Generic;
using AipmRegister.Core.Models;
using AipmRegister.Core.Notification;
using AipmRegister.Core.Orchestration;
using AipmRegister.Core.Wifi;
using AipmRegister.Gui.Localization;
using AipmRegister.Gui.Notification;
using AipmRegister.Gui.ViewModels;
using AipmRegister.Gui.Wizard;

namespace AipmRegister.Gui.Tests.ViewModels;

public sealed class RegisteringViewModelTests
{
    [Fact]
    public async Task RunAsync_EnablesRetry_WhenPollingEndsWithoutSuccess()
    {
        var orchestrator = new StubRegistrationOrchestrator();
        var vm = CreateViewModel(orchestrator);

        await vm.RunAsync();

        Assert.True(vm.IsRetryAvailable);
        Assert.True(vm.RetryCommand.CanExecute(null));
        Assert.Equal("등록 실패 : 장치를 초기화 후 다시 등록해주세요.", vm.StatusText);
    }

    [Fact]
    public async Task RetryCommand_RunsRegistrationAgain()
    {
        var orchestrator = new StubRegistrationOrchestrator();
        var vm = CreateViewModel(orchestrator);

        await vm.RunAsync();
        vm.RetryCommand.Execute(null);
        await vm.RetryCommand.ExecutionTask!;

        Assert.Equal(2, orchestrator.HandOffCalls);
    }

    private static RegisteringViewModel CreateViewModel(StubRegistrationOrchestrator orchestrator)
    {
        var state = new WizardState
        {
            Account = new Account("user-1", "pc-key-1", "37.0", "127.0"),
            Product = ProductCatalog.All[0],
            WifiAdapter = new StubWifiAdapter(),
            HomeSsid = "home",
            HomePassword = "password",
            DeviceHotspotSsid = "DWD-S120_001122",
        };

        return new RegisteringViewModel(
            orchestrator,
            new NoopNotifier(),
            new UiNotifier(),
            new NoopNavigator(),
            state,
            new AipmRegister.Gui.Localization.Localization());
    }

    private sealed class StubRegistrationOrchestrator : IRegistrationOrchestrator
    {
        public int HandOffCalls { get; private set; }

        public Task<RegistrationResult> RunAsync(
            RegistrationRequest request,
            IWifiAdapter wifi,
            CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<Account?> ExchangeAuthCodeAsync(
            string authCode8Digits,
            CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<DeviceModelInfo> SendDeviceSettingsAsync(
            Account account,
            ProductDefinition picked,
            string deviceHotspotSsid,
            string homeSsid,
            string homePassword,
            string deviceTcpHost,
            int deviceTcpPort,
            CancellationToken ct = default) =>
            throw new NotSupportedException();

        public Task<DeviceModelInfo> HandOffToDeviceAsync(
            Account account,
            ProductDefinition picked,
            RegistrationRequest request,
            IWifiAdapter wifi,
            CancellationToken ct = default)
        {
            HandOffCalls++;
            return Task.FromResult(new DeviceModelInfo("001122", "B530_W", "DWM-B530_W-001122"));
        }

        public IAsyncEnumerable<ControlCheckTick> PollRegistrationAsync(
            Account account,
            string deviceId,
            int maxAttempts,
            TimeSpan pollInterval,
            CancellationToken ct = default) =>
            EmptyTicks();

        private static async IAsyncEnumerable<ControlCheckTick> EmptyTicks()
        {
            await Task.Yield();
            yield break;
        }
    }

    private sealed class StubWifiAdapter : IWifiAdapter
    {
        public Task<IReadOnlyList<WifiNetwork>> ScanAsync(CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<WifiNetwork>>(Array.Empty<WifiNetwork>());

        public Task ConnectAsync(
            string ssid,
            string password,
            WifiSecurity security,
            CancellationToken ct = default) =>
            Task.CompletedTask;

        public Task DisconnectAndForgetAsync(string ssid, CancellationToken ct = default) =>
            Task.CompletedTask;
    }

    private sealed class NoopNotifier : IUserNotifier
    {
        public void Info(string message) { }
        public void Warn(string message) { }
        public void Error(string message, Exception? cause = null) { }
        public void Progress(RegistrationStage stage, string message) { }
    }

    private sealed class NoopNavigator : IWizardNavigator
    {
        public void Go(WizardStep step) { }
    }
}
