using System.Collections.ObjectModel;
using AipmRegister.Core.Orchestration;
using AipmRegister.Gui.Notification;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AipmRegister.Gui.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IRegistrationOrchestrator _orchestrator;
    private readonly UiNotifier _notifier;
    private CancellationTokenSource? _cts;

    public MainViewModel(IRegistrationOrchestrator orchestrator, UiNotifier notifier)
    {
        _orchestrator = orchestrator;
        _notifier = notifier;
        Logs = notifier.Entries;
    }

    [ObservableProperty] private string  authCode             = string.Empty;
    [ObservableProperty] private string  deviceHotspotSsid    = string.Empty;
    [ObservableProperty] private string  deviceHotspotPassword= string.Empty;
    [ObservableProperty] private string  homeSsid             = string.Empty;
    [ObservableProperty] private string  homePassword         = string.Empty;
    [ObservableProperty] private string  deviceTcpHost        = "192.168.4.1";
    [ObservableProperty] private int     deviceTcpPort        = 5000;
    [ObservableProperty] private int     maxAttempts          = 30;
    [ObservableProperty] private int     pollSeconds          = 2;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsIdle))]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    [NotifyCanExecuteChangedFor(nameof(CancelCommand))]
    private bool isBusy;

    public bool IsIdle => !IsBusy;

    [ObservableProperty] private string resultStatus  = string.Empty;
    [ObservableProperty] private string resultMessage = string.Empty;

    public ObservableCollection<LogEntry> Logs { get; }

    [RelayCommand(CanExecute = nameof(CanRegister))]
    private async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(AuthCode) ||
            string.IsNullOrWhiteSpace(HomeSsid) ||
            string.IsNullOrWhiteSpace(DeviceHotspotSsid))
        {
            _notifier.Warn("Auth code, home SSID, and device hotspot SSID are required.");
            return;
        }

        IsBusy = true;
        ResultStatus = "running";
        ResultMessage = string.Empty;
        Logs.Clear();

        _cts = new CancellationTokenSource();
        try
        {
            var request = new RegistrationRequest(
                AuthCode8Digits:        AuthCode,
                HomeSsid:               HomeSsid,
                HomePassword:           HomePassword,
                DeviceHotspotSsid:      DeviceHotspotSsid,
                DeviceHotspotPassword:  DeviceHotspotPassword,
                DeviceTcpHost:          DeviceTcpHost,
                DeviceTcpPort:          DeviceTcpPort,
                MaxControlCheckAttempts:MaxAttempts,
                PollInterval:           TimeSpan.FromSeconds(PollSeconds));

            var result = await _orchestrator.RunAsync(request, _cts.Token);

            ResultStatus = result.Status.ToString();
            ResultMessage = $"user_id={result.UserId ?? "-"}  device_id={result.DeviceId ?? "-"}  msg={result.Message ?? "-"}";
        }
        catch (OperationCanceledException)
        {
            ResultStatus = "cancelled";
        }
        catch (Exception ex)
        {
            ResultStatus = "exception";
            ResultMessage = ex.Message;
            _notifier.Error("Unhandled exception during registration.", ex);
        }
        finally
        {
            IsBusy = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    private bool CanRegister() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel() => _cts?.Cancel();

    private bool CanCancel() => IsBusy;
}
