using System.Collections.ObjectModel;
using AipmRegister.Core.Models;
using AipmRegister.Core.Notification;
using AipmRegister.Core.Wifi;
using AipmRegister.Gui.Wizard;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AipmRegister.Gui.ViewModels;

public partial class DevicePickerViewModel : ObservableObject
{
    private readonly IUserNotifier _notifier;
    private readonly IWizardNavigator _nav;
    private readonly WizardState _state;

    public DevicePickerViewModel(
        IUserNotifier notifier,
        IWizardNavigator nav,
        WizardState state)
    {
        _notifier = notifier;
        _nav = nav;
        _state = state;
    }

    public ObservableCollection<DeviceCandidate> Devices { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private DeviceCandidate? selected;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private bool isBusy;

    partial void OnSelectedChanged(DeviceCandidate? value)
    {
        if (value is not null)
        {
            _state.DeviceHotspotSsid = value.Ssid;
            _state.DeviceMac = value.Mac;
        }
    }

    /// Called by MainViewModel whenever the user lands on step 4/5, so the
    /// list reflects the current product pick.
    public async Task PrimeAsync()
    {
        Devices.Clear();
        await RefreshAsync();
    }

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshAsync()
    {
        if (_state.Product is null || _state.WifiAdapter is null) return;
        IsBusy = true;
        try
        {
            Devices.Clear();
            var found = await _state.WifiAdapter.ScanAsync();
            foreach (var n in found
                .Where(n => _state.Product.IsHotspotOf(n.Ssid))
                .OrderByDescending(n => n.SignalQuality))
            {
                Devices.Add(new DeviceCandidate(
                    Ssid: n.Ssid,
                    Mac: ExtractMac(n.Ssid),
                    SignalQuality: n.SignalQuality));
            }
        }
        catch (Exception ex)
        {
            _notifier.Error("Device scan failed.", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanRefresh() => !IsBusy && _state.Product is not null;

    [RelayCommand(CanExecute = nameof(CanRegister))]
    private void Register() => _nav.Go(WizardStep.Registering);

    private bool CanRegister() => !IsBusy && Selected is not null;

    [RelayCommand]
    private void Back() => _nav.Go(WizardStep.ProductPicker);

    /// "DWD-S120_3b12b9" → "3b12b9". Some SSIDs put the MAC tail after `_`,
    /// others use `-`; treat both.
    private static string ExtractMac(string ssid)
    {
        var idx = ssid.LastIndexOfAny(new[] { '_', '-' });
        return idx >= 0 && idx < ssid.Length - 1 ? ssid[(idx + 1)..] : ssid;
    }
}

public sealed record DeviceCandidate(string Ssid, string Mac, int SignalQuality);
