using AipmRegister.Core.Models;
using AipmRegister.Core.Notification;
using AipmRegister.Core.Orchestration;
using AipmRegister.Core.Wifi;
using AipmRegister.Gui.Localization;
using AipmRegister.Gui.Wizard;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AipmRegister.Gui.ViewModels;

public partial class RegisteringViewModel : ObservableObject
{
    private readonly IRegistrationOrchestrator _orchestrator;
    private readonly IUserNotifier _notifier;
    private readonly IWizardNavigator _nav;
    private readonly WizardState _state;

    public RegisteringViewModel(
        IRegistrationOrchestrator orchestrator,
        IUserNotifier notifier,
        IWizardNavigator nav,
        WizardState state)
    {
        _orchestrator = orchestrator;
        _notifier = notifier;
        _nav = nav;
        _state = state;
    }

    [ObservableProperty] private int    progressValue;
    [ObservableProperty] private int    progressMaximum = 20;
    [ObservableProperty] private string statusText = string.Empty;
    [ObservableProperty] private bool   succeeded;
    [ObservableProperty] private bool   isBusy;

    private CancellationTokenSource? _cts;

    /// Called by MainViewModel when the user enters step 5/5.
    public async Task RunAsync()
    {
        if (_state.Account is null || _state.Product is null || _state.WifiAdapter is null) return;

        IsBusy = true;
        ProgressValue = 0;
        StatusText = L.Instance["Step5.InProgress"];
        Succeeded = false;
        _cts = new CancellationTokenSource();

        try
        {
            // Steps a-c: hotspot join → TCP push → rejoin home. The
            // orchestrator's HandOffToDeviceAsync owns the flow so the
            // CLI and GUI share the same logic; this VM only observes
            // progress + decides UI state.
            var request = BuildRequest();
            var info = await _orchestrator.HandOffToDeviceAsync(
                _state.Account, _state.Product, request, _state.WifiAdapter, _cts.Token);
            _state.DeviceId = info.DeviceId;
            _state.DeviceMac = info.Mac;

            // Step d: poll until terminal outcome
            await foreach (var tick in _orchestrator.PollRegistrationAsync(
                _state.Account, info.DeviceId,
                ProgressMaximum, TimeSpan.FromSeconds(2), _cts.Token))
            {
                ProgressValue = tick.Attempt;

                switch (tick.Outcome)
                {
                    case ControlCheckOutcome.Success:
                        Succeeded = true;
                        ProgressValue = ProgressMaximum;
                        StatusText = L.Instance["Step5.Done"];
                        return;
                    case ControlCheckOutcome.AlreadyRegistered:
                        StatusText = L.Instance["Error.AlreadyRegistered"];
                        return;
                    case ControlCheckOutcome.AuthCodeExpired:
                        StatusText = L.Instance["Error.AuthExpired"];
                        return;
                    case ControlCheckOutcome.NotRegisteredExceededAttempts:
                        StatusText = L.Instance["Error.NotRegistered"];
                        return;
                }
            }
            // Loop ended without terminal — treat as not responding.
            StatusText = L.Instance["Error.NotRegistered"];
        }
        catch (OperationCanceledException)
        {
            StatusText = "cancelled";
        }
        catch (Exception ex)
        {
            StatusText = ex.Message;
            _notifier.Error("Registration failed.", ex);
        }
        finally
        {
            IsBusy = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private RegistrationRequest BuildRequest() => new(
        AuthCode8Digits:        _state.AuthCode,
        HomeSsid:               _state.HomeSsid,
        HomePassword:           _state.HomePassword,
        DeviceHotspotSsid:      _state.DeviceHotspotSsid,
        DeviceHotspotPassword:  string.Empty,
        DeviceTcpHost:          _state.DeviceTcpHost,
        DeviceTcpPort:          _state.DeviceTcpPort,
        MaxControlCheckAttempts:ProgressMaximum,
        PollInterval:           TimeSpan.FromSeconds(2));

    [RelayCommand]
    private void Back()
    {
        _cts?.Cancel();
        _nav.Go(WizardStep.ProductPicker);
    }

    [RelayCommand]
    private void Unbind()
    {
        _cts?.Cancel();
        _state.Account = null;
        _state.AuthCode = string.Empty;
        _nav.Go(WizardStep.AuthCode);
    }
}
