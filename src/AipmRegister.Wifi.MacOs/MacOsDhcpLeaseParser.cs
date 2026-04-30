namespace AipmRegister.Wifi.MacOs;

/// Parses `ipconfig getpacket <iface>` output. On macOS this is the most
/// direct way to recover the router option DHCP assigned to a specific Wi-Fi
/// interface after joining an IoT device hotspot.
public static class MacOsDhcpLeaseParser
{
    private const string RouterPrefix = "router (ip_mult):";

    public static string? ParseRouter(string output)
    {
        foreach (var rawLine in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var line = rawLine.Trim();
            if (!line.StartsWith(RouterPrefix, StringComparison.Ordinal)) continue;

            var open = line.IndexOf('{', RouterPrefix.Length);
            var close = line.IndexOf('}', open + 1);
            if (open < 0 || close <= open) return null;

            var first = line[(open + 1)..close]
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault();
            return string.IsNullOrWhiteSpace(first) ? null : first;
        }

        return null;
    }
}
