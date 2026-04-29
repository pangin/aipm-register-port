using System.Runtime.Versioning;

namespace AipmRegister.Wifi.Linux;

/// Pure parser for `iw dev` output, separated from the I/O so it's
/// unit-testable without a Linux host.
///
/// `iw dev` on a multi-radio system prints:
///
///   phy#0
///       Interface wlan0
///           ifindex 3
///           addr 00:11:22:33:44:55
///           type managed
///           ...
///   phy#1
///       Interface wlan1
///           ...
///
/// We only need the interface names — every leading-whitespace line that
/// starts with "Interface ".
[SupportedOSPlatform("linux")]
internal static class WirelessInterfaceParser
{
    private const string InterfacePrefix = "Interface ";

    public static IReadOnlyList<string> ParseIwDev(string output)
    {
        if (string.IsNullOrWhiteSpace(output)) return Array.Empty<string>();

        var names = new List<string>();
        foreach (var rawLine in output.Split('\n'))
        {
            var trimmed = rawLine.Trim();
            if (!trimmed.StartsWith(InterfacePrefix, StringComparison.Ordinal)) continue;

            var name = trimmed.Substring(InterfacePrefix.Length).Trim();
            if (!string.IsNullOrEmpty(name) && !names.Contains(name, StringComparer.Ordinal))
            {
                names.Add(name);
            }
        }
        return names;
    }
}
