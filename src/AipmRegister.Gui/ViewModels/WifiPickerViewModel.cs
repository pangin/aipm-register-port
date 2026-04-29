using System.Collections.ObjectModel;
using AipmRegister.Core.Notification;
using AipmRegister.Core.Wifi;
using AipmRegister.Gui.Wizard;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AipmRegister.Gui.ViewModels;

public partial class WifiPickerViewModel : ObservableObject
{
    private readonly IWifiAdapter _wifi;
    private readonly IUserNotifier _notifier;
    private readonly IWizardNavigator _nav;
    private readonly WizardState _state;

    public WifiPickerViewModel(
        IWifiAdapter wifi,
        IUserNotifier notifier,
        IWizardNavigator nav,
        WizardState state)
    {
        _wifi = wifi;
        _notifier = notifier;
        _nav = nav;
        _state = state;
    }

    public ObservableCollection<WifiNetwork> Networks { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private WifiNetwork? selected;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    private string homePassword = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    private bool isBusy;

    partial void OnSelectedChanged(WifiNetwork? value)
    {
        if (value is not null)
        {
            _state.HomeSsid = value.Ssid;
        }
    }

    partial void OnHomePasswordChanged(string value) => _state.HomePassword = value;

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshAsync()
    {
        IsBusy = true;
        try
        {
            Networks.Clear();
            var found = await _wifi.ScanAsync();
            foreach (var n in found.OrderByDescending(x => x.SignalQuality))
            {
                Networks.Add(n);
            }
        }
        catch (Exception ex)
        {
            _notifier.Error("Wi-Fi scan failed.", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanRefresh() => !IsBusy;

    [RelayCommand(CanExecute = nameof(CanConnect))]
    private async Task ConnectAsync()
    {
        IsBusy = true;
        try
        {
            await _wifi.ConnectAsync(_state.HomeSsid, _state.HomePassword,
                Selected?.Security ?? WifiSecurity.Wpa2Personal);
            _nav.Go(WizardStep.AuthCode);
        }
        catch (Exception ex)
        {
            _notifier.Error("Failed to connect.", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanConnect()
        => !IsBusy && !string.IsNullOrWhiteSpace(_state.HomeSsid);
}
