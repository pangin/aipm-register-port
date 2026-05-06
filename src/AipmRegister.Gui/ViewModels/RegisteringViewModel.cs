using System.Collections.ObjectModel;
using AipmRegister.Core.Models;
using AipmRegister.Core.Notification;
using AipmRegister.Core.Orchestration;
using AipmRegister.Core.Wifi;
using AipmRegister.Gui.Localization;
using AipmRegister.Gui.Notification;
using AipmRegister.Gui.Wizard;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AipmRegister.Gui.ViewModels;

public partial class RegisteringViewModel : ObservableObject
{
    private readonly IRegistrationOrchestrator _orchestrator;
    private readonly IUserNotifier _notifier;
    private readonly UiNotifier _ui;
    private readonly IWizardNavigator _nav;
    private readonly WizardState _state;
    private readonly ILocalization _l;

    public RegisteringViewModel(
        IRegistrationOrchestrator orchestrator,
        IUserNotifier notifier,
        UiNotifier ui,
        IWizardNavigator nav,
        WizardState state,
        ILocalization l)
    {
        _orchestrator = orchestrator;
        _notifier = notifier;
        _ui = ui;
        _nav = nav;
        _state = state;
        _l = l;
    }

    /// Live feed of orchestrator/notifier events for the "자세히 보기"
    /// terminal-style panel on the Step 5/5 view.
    public ObservableCollection<LogEntry> LogEntries => _ui.Entries;

    [ObservableProperty] private int    progressValue;
    [ObservableProperty] private int    progressMaximum = 20;
    [ObservableProperty] private string statusText = string.Empty;
    [ObservableProperty] private bool   succeeded;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RetryCommand))]
    private bool isBusy;

    [ObservableProperty] private bool   isLogVisible;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RetryCommand))]
    private bool isRetryAvailable;

    [RelayCommand]
    private void ToggleLog() => IsLogVisible = !IsLogVisible;

    private CancellationTokenSource? _cts;

    /// Called by MainViewModel when the user enters step 5/5.
    public async Task RunAsync()
    {
        if (IsBusy) return;
        if (_state.Account is null || _state.Product is null || _state.WifiAdapter is null) return;

        IsBusy = true;
        IsRetryAvailable = false;
        ProgressValue = 0;
        StatusText = _l["Step5.InProgress"];
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
                        StatusText = _l["Step5.Done"];
                        return;
                    case ControlCheckOutcome.AlreadyRegistered:
                        StatusText = _l["Error.AlreadyRegistered"];
                        IsRetryAvailable = true;
                        return;
                    case ControlCheckOutcome.AuthCodeExpired:
                        StatusText = _l["Error.AuthExpired"];
                        return;
                    // NotRegistered isn't terminal here — the orchestrator's
                    // PollRegistrationAsync counts repeats and ends the
                    // stream once the count crosses the wall-clock-equivalent
                    // threshold (frmMain.cs:2366). The fall-through below
                    // shows "Error.NotRegistered" once the loop ends without
                    // a terminal tick.
                }
            }
            // Loop ended without terminal — treat as not responding.
            StatusText = _l["Error.NotRegistered"];
            IsRetryAvailable = true;
        }
        catch (OperationCanceledException)
        {
            StatusText = "cancelled";
        }
        catch (Exception ex)
        {
            StatusText = ex.Message;
            IsRetryAvailable = true;
            _notifier.Error("Registration failed.", ex);
        }
        finally
        {
            IsBusy = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private bool CanRetry() => !IsBusy && IsRetryAvailable;

    [RelayCommand(CanExecute = nameof(CanRetry))]
    private Task RetryAsync() => RunAsync();

    private RegistrationRequest BuildRequest() => new(
        AuthCode8Digits:        _state.AuthCode,
        HomeSsid:               _state.HomeSsid,
        HomePassword:           _state.HomePassword,
        DeviceHotspotSsid:      _state.DeviceHotspotSsid,
        DeviceHotspotPassword:  string.Empty,
        DeviceTcpHost:          _state.DeviceTcpHost,
        DeviceTcpPort:          _state.DeviceTcpPort,
        MaxControlCheckAttempts:ProgressMaximum,
        PollInterval:           TimeSpan.FromSeconds(2),
        HomeSecurity:           _state.HomeSecurity);

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
