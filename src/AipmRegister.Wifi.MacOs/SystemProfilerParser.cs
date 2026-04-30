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
    public static IReadOnlyList<WifiNetwork> Parse(string plistXml, string? interfaceName = null)
    {
        var doc = XDocument.Parse(plistXml);

        var results = new List<WifiNetwork>();
        var seen = new HashSet<(string Ssid, string Band, WifiSecurity Security)>();

        foreach (var iface in CandidateInterfaceDictionaries(doc, interfaceName))
        {
            var current = NextDictAfterKey(iface, "spairport_current_network_information");
            if (current is not null) AddNetwork(current);

            var localList = NextArrayAfterKey(iface, "spairport_airport_other_local_wireless_networks");
            if (localList is null) continue;

            foreach (var net in localList.Elements("dict"))
            {
                AddNetwork(net);
            }
        }
        return results;

        void AddNetwork(XElement net)
        {
            var ssid        = StringAfterKey(net, "_name");
            var channel     = StringAfterKey(net, "spairport_network_channel");
            var security    = MapSecurity(StringAfterKey(net, "spairport_security_mode"));
            var band        = WifiBandClassifier.FromChannel(channel);
            var signalNoise = StringAfterKey(net, "spairport_signal_noise");

            if (string.IsNullOrWhiteSpace(ssid)) return;
            if (!seen.Add((ssid!, band, security))) return;

            results.Add(new WifiNetwork(
                Ssid:          ssid!,
                SignalQuality: SignalNoiseToQuality(signalNoise),
                Security:      security,
                Band:          band));
        }
    }

    private static IEnumerable<XElement> CandidateInterfaceDictionaries(XDocument doc, string? interfaceName)
    {
        foreach (var dict in doc.Descendants("dict"))
        {
            if (NextArrayAfterKey(dict, "spairport_airport_other_local_wireless_networks") is null
                && NextDictAfterKey(dict, "spairport_current_network_information") is null)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(interfaceName)
                && !string.Equals(StringAfterKey(dict, "_name"), interfaceName, StringComparison.Ordinal))
            {
                continue;
            }

            yield return dict;
        }
    }

    private static XElement? NextArrayAfterKey(XElement parent, string keyName)
    {
        var el = NextElementAfterKey(parent, keyName);
        return el?.Name.LocalName == "array" ? el : null;
    }

    private static XElement? NextDictAfterKey(XElement parent, string keyName)
    {
        var el = NextElementAfterKey(parent, keyName);
        return el?.Name.LocalName == "dict" ? el : null;
    }

    private static string? StringAfterKey(XElement parent, string keyName)
        => NextElementAfterKey(parent, keyName)?.Value;

    private static XElement? NextElementAfterKey(XElement parent, string keyName)
    {
        XElement? prevKey = null;
        foreach (var el in parent.Elements())
        {
            if (el.Name.LocalName == "key" && el.Value == keyName)
            {
                prevKey = el;
                continue;
            }
            if (prevKey is not null) return el;
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
