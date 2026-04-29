using System.Xml.Linq;
using AipmRegister.Core.Wifi;

namespace AipmRegister.Wifi.MacOs;

/// Parses the plist XML from `system_profiler SPAirPortDataType -xml`.
///
/// macOS 14 deprecated `airport -s` so this is now the supported path for
/// listing nearby APs. The output is a deeply-nested plist; we descend down
/// to the `Other Local Wi-Fi Networks` array under each interface entry.
///
/// Pure parsing — no Process.Start. Wrapped behind a public surface so the
/// macOS test project can pin behavior on any host (including Windows CI).
public static class SystemProfilerParser
{
    public static IReadOnlyList<WifiNetwork> Parse(string plistXml)
    {
        var doc = XDocument.Parse(plistXml);
        var root = doc.Root?.Element("array");
        if (root is null) return Array.Empty<WifiNetwork>();

        var results = new List<WifiNetwork>();
        // The top-level array contains one dict (with key "_items" → array → interfaces)
        foreach (var topDict in root.Elements("dict"))
        {
            var topItems = NextArrayAfterKey(topDict, "_items");
            if (topItems is null) continue;

            foreach (var iface in topItems.Elements("dict"))
            {
                // Each interface dict has spairport_airport_other_local_wireless_networks
                var localList = NextArrayAfterKey(iface, "spairport_airport_other_local_wireless_networks");
                if (localList is null) continue;

                foreach (var net in localList.Elements("dict"))
                {
                    var ssid       = StringAfterKey(net, "_name");
                    var channel    = StringAfterKey(net, "spairport_network_channel");
                    var security   = StringAfterKey(net, "spairport_security_mode");
                    var signalNoise = StringAfterKey(net, "spairport_signal_noise");

                    if (string.IsNullOrWhiteSpace(ssid)) continue;

                    results.Add(new WifiNetwork(
                        Ssid:          ssid!,
                        SignalQuality: SignalNoiseToQuality(signalNoise),
                        Security:      MapSecurity(security),
                        Band:          ChannelToBand(channel)));
                }
            }
        }
        return results;
    }

    private static XElement? NextArrayAfterKey(XElement parent, string keyName)
    {
        XElement? prevKey = null;
        foreach (var el in parent.Elements())
        {
            if (el.Name.LocalName == "key" && el.Value == keyName)
            {
                prevKey = el;
                continue;
            }
            if (prevKey is not null && el.Name.LocalName == "array") return el;
            prevKey = null;
        }
        return null;
    }

    private static string? StringAfterKey(XElement parent, string keyName)
    {
        XElement? prevKey = null;
        foreach (var el in parent.Elements())
        {
            if (el.Name.LocalName == "key" && el.Value == keyName)
            {
                prevKey = el;
                continue;
            }
            if (prevKey is not null) return el.Value;
            prevKey = null;
        }
        return null;
    }

    /// "spairport_security_mode_wpa2_personal" → Wpa2Personal, etc.
    public static WifiSecurity MapSecurity(string? mode)
    {
        if (string.IsNullOrEmpty(mode)) return WifiSecurity.Open;
        var lower = mode.ToLowerInvariant();
        if (lower.Contains("wpa3")) return WifiSecurity.Wpa3Personal;
        if (lower.Contains("wpa2")) return WifiSecurity.Wpa2Personal;
        if (lower.Contains("wpa"))  return WifiSecurity.WpaPersonal;
        if (lower.Contains("wep"))  return WifiSecurity.Wep;
        if (lower.Contains("none") || lower.Contains("open")) return WifiSecurity.Open;
        return WifiSecurity.Open;
    }

    /// Channel 1-14 -> 2.4G, 32-177 -> 5G, 1-233 -> 6G (overlap, but
    /// system_profiler tags 6E channels with " (6 GHz)" suffix in newer
    /// macOS versions, so we sniff for that first).
    public static string ChannelToBand(string? channel)
    {
        if (string.IsNullOrEmpty(channel)) return "?";
        if (channel.Contains("6 GHz", StringComparison.OrdinalIgnoreCase)) return "6G";
        if (channel.Contains("5 GHz", StringComparison.OrdinalIgnoreCase)) return "5G";
        if (channel.Contains("2.4",   StringComparison.OrdinalIgnoreCase)) return "2.4G";

        // Numeric channel only — fall back to canonical bands.
        var token = channel.Split(',', '(', ' ')[0].Trim();
        if (int.TryParse(token, out var n))
        {
            return n switch
            {
                >= 1 and <= 14   => "2.4G",
                >= 32 and <= 196 => "5G",
                >= 1 and <= 233  => "6G",     // 6 GHz numeric channels overlap with 2.4G
                _                => "?",
            };
        }
        return "?";
    }

    /// "-45 dBm / -90 dBm" → 100. We treat the first number (signal) the
    /// same way Linux does.
    public static int SignalNoiseToQuality(string? signalNoise)
    {
        if (string.IsNullOrEmpty(signalNoise)) return 0;
        var first = signalNoise.Split('/', ',', ' ')[0].Trim();
        if (int.TryParse(first, out var dbm))
        {
            if (dbm >= -50)  return 100;
            if (dbm <= -100) return 0;
            return 2 * (dbm + 100);
        }
        return 0;
    }
}
