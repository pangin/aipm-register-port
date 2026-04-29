using AipmRegister.Core.Wifi;

namespace AipmRegister.Wifi.Linux;

/// Parses the textual output of the wpa_supplicant SCAN_RESULTS command.
/// Format (tab-separated):
///     bssid / frequency / signal level / flags / ssid
/// flags is a bracketed-token list, e.g.
///     [WPA2-PSK-CCMP][ESS]  /  [WPA-PSK-CCMP+TKIP][WPA2-PSK-CCMP][ESS]
///
/// Pure parsing — no IO. Lives behind a public surface so the CLI/GUI tests
/// can pin behavior without spinning up wpa_supplicant.
public static class WpaScanResultParser
{
    public static IReadOnlyList<WifiNetwork> Parse(string scanResults)
    {
        var lines = scanResults.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var list = new List<WifiNetwork>(lines.Length);

        foreach (var line in lines)
        {
            // first line is the column header — skip it
            if (line.StartsWith("bssid", StringComparison.OrdinalIgnoreCase)) continue;

            var parts = line.Split('\t');
            if (parts.Length < 5) continue;

            var freq    = int.TryParse(parts[1], out var f) ? f : 0;
            var signal  = int.TryParse(parts[2], out var s) ? s : 0;
            var flags   = parts[3];
            var ssid    = parts[4];

            if (string.IsNullOrWhiteSpace(ssid)) continue;

            list.Add(new WifiNetwork(
                Ssid:          ssid,
                SignalQuality: ClampSignalToQuality(signal),
                Security:      ClassifySecurity(flags),
                Band:          WifiBandClassifier.FromFrequencyMhz(freq)));
        }

        return list;
    }

    /// wpa_supplicant gives signal as dBm (e.g. -42). Map to a 0-100 quality
    /// the way Windows does: -50 dBm == 100, -100 dBm == 0.
    public static int ClampSignalToQuality(int dbm)
    {
        if (dbm >= -50)  return 100;
        if (dbm <= -100) return 0;
        return 2 * (dbm + 100);
    }

    public static WifiSecurity ClassifySecurity(string flags)
    {
        // Order matters — WPA3 takes precedence over WPA2, etc.
        if (flags.Contains("SAE", StringComparison.Ordinal)
            || flags.Contains("WPA3", StringComparison.Ordinal))   return WifiSecurity.Wpa3Personal;
        if (flags.Contains("WPA2", StringComparison.Ordinal))      return WifiSecurity.Wpa2Personal;
        if (flags.Contains("WPA", StringComparison.Ordinal))       return WifiSecurity.WpaPersonal;
        if (flags.Contains("WEP", StringComparison.Ordinal))       return WifiSecurity.Wep;
        return WifiSecurity.Open;
    }
}
