namespace AipmRegister.Core.Wifi;

/// OS abstraction over the platform Wi-Fi stack. Win32 implementation lives in
/// the Windows-specific code path; Linux (NetworkManager / wpa_supplicant) and
/// macOS (CoreWLAN) are stretch targets for follow-up commits.
public interface IWifiAdapter
{
    /// Lists currently visible access points (BSS-level results, with link
    /// quality and security info).
    Task<IReadOnlyList<WifiNetwork>> ScanAsync(CancellationToken ct = default);

    /// Installs a temporary profile and connects to the named SSID. Returns
    /// only after the OS reports the connection as established (or the token
    /// is cancelled).
    Task ConnectAsync(string ssid, string password, WifiSecurity security, CancellationToken ct = default);

    /// Removes the profile named matching the SSID. Best-effort.
    Task DisconnectAndForgetAsync(string ssid, CancellationToken ct = default);
}

public enum WifiSecurity
{
    Open,
    Wep,
    WpaPersonal,
    Wpa2Personal,
    Wpa3Personal,
}

public sealed record WifiNetwork(
    string Ssid,
    int SignalQuality,
    WifiSecurity Security,
    string Band)
{
    /// True when this AP can host a DAWON IoT device pairing — the device
    /// MCUs only support 2.4GHz with WEP/WPA-Personal/WPA2-Personal. The
    /// original UI paints the row green when this is true and orange
    /// otherwise (frmMain.cs:1267-1275).
    public bool IsRecommended =>
        Band.StartsWith("2.4", StringComparison.Ordinal) || Band == "2G"
            ? Security is WifiSecurity.Wep
                or WifiSecurity.WpaPersonal
                or WifiSecurity.Wpa2Personal
            : false;
}
