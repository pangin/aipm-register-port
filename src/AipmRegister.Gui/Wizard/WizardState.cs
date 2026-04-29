using AipmRegister.Core.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AipmRegister.Gui.Wizard;

/// Shared state every step of the wizard reads/writes through. Mirrors
/// frmMain.cs's instance fields (m_d=user_id, m_e=pc_key, m_B=primary
/// SSID prefix, m_8=home SSID, m_9=home password, m_A=device MAC, etc.)
/// but with explicit names.
public partial class WizardState : ObservableObject
{
    [ObservableProperty] private string  homeSsid           = string.Empty;
    [ObservableProperty] private string  homePassword       = string.Empty;
    [ObservableProperty] private string  authCode           = string.Empty;

    /// Filled in step 2/5 after `getPckey`.
    [ObservableProperty] private Account? account;

    /// Picked in step 3/5.
    [ObservableProperty] private ProductDefinition? product;

    /// Picked in step 4/5 (one of the device hotspots filtered by product).
    [ObservableProperty] private string  deviceHotspotSsid  = string.Empty;
    [ObservableProperty] private string  deviceMac          = string.Empty;

    /// Composed in step 5/5 once the TCP hand-off succeeds.
    [ObservableProperty] private string  deviceId           = string.Empty;

    [ObservableProperty] private string  deviceTcpHost      = "192.168.4.1";
    [ObservableProperty] private int     deviceTcpPort      = 5000;
}
