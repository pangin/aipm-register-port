using System.Collections.ObjectModel;
using AipmRegister.Core.Notification;
using AipmRegister.Core.Wifi;
using AipmRegister.Gui.Wizard;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AipmRegister.Gui.ViewModels;

public partial class WifiPickerViewModel : ObservableObject
{
    private readonly IWifiInterfaceEnumerator _enumerator;
    private readonly IWifiAdapterFactory _factory;
    private readonly IUserNotifier _notifier;
    private readonly IWizardNavigator _nav;
    private readonly WizardState _state;

    public WifiPickerViewModel(
        IWifiInterfaceEnumerator enumerator,
        IWifiAdapterFactory factory,
        IUserNotifier notifier,
        IWizardNavigator nav,
        WizardState state)
    {
        _enumerator = enumerator;
        _factory = factory;
        _notifier = notifier;
        _nav = nav;
        _state = state;
    }

    public ObservableCollection<WifiInterface> Interfaces { get; } = new();
    public ObservableCollection<WifiNetwork> Networks { get; } = new();

    /// True only when there is more than one wireless adapter to pick
    /// from. The view binds this to the dropdown's Visibility — keeps the
    /// common single-radio laptop case visually unchanged.
    public bool ShowAdapterPicker => Interfaces.Count > 1;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    private WifiInterface? selectedInterface;

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

    partial void OnSelectedInterfaceChanged(WifiInterface? value)
    {
        if (value is null) return;
        _state.WifiInterface = value;
        _state.WifiAdapter = _factory.Create(value);
    }

    partial void OnSelectedChanged(WifiNetwork? value)
    {
        if (value is not null)
        {
            _state.HomeSsid = value.Ssid;
            _state.HomeSecurity = value.Security;
        }
    }

    partial void OnHomePasswordChanged(string value) => _state.HomePassword = value;

    /// Called by MainViewModel when the wizard advances onto step 1/5.
    /// Enumerates wireless interfaces, auto-picks when N=1, otherwise
    /// surfaces the dropdown so the user can disambiguate.
    public async Task PrimeAsync()
    {
        if (Interfaces.Count > 0) return; // already enumerated this session

        try
        {
            var found = await _enumerator.EnumerateAsync();
            Interfaces.Clear();
            foreach (var iface in found) Interfaces.Add(iface);
            OnPropertyChanged(nameof(ShowAdapterPicker));

            // N=1 → silent auto-pick (preserves the single-radio laptop UX).
            if (Interfaces.Count == 1) SelectedInterface = Interfaces[0];
        }
        catch (Exception ex)
        {
            _notifier.Error("Wi-Fi adapter enumeration failed.", ex);
        }
    }

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshAsync()
    {
        if (_state.WifiAdapter is null) return;
        IsBusy = true;
        try
        {
            Networks.Clear();
            var found = await _state.WifiAdapter.ScanAsync();
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

    private bool CanRefresh() => !IsBusy && _state.WifiAdapter is not null;

    [RelayCommand(CanExecute = nameof(CanConnect))]
    private async Task ConnectAsync()
    {
        if (_state.WifiAdapter is null) return;
        IsBusy = true;
        try
        {
            await _state.WifiAdapter.ConnectAsync(
                _state.HomeSsid, _state.HomePassword,
                _state.HomeSecurity);
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
        => !IsBusy
           && _state.WifiAdapter is not null
           && !string.IsNullOrWhiteSpace(_state.HomeSsid);
}
