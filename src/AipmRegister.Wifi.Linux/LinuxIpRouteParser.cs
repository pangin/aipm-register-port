namespace AipmRegister.Wifi.Linux;

/// Pure parser for the output of `ip route show default dev &lt;iface&gt;`.
///
/// Typical line:
/// <code>default via 192.168.4.1 dev wlan0 proto dhcp metric 600</code>
///
/// Mirrors the role of <c>MacOsDhcpLeaseParser</c> on macOS: the IO
/// (running the actual <c>ip</c> command) lives behind
/// <see cref="LinuxWifiAdapter"/> + <c>IProcessRunner</c>, while this
/// pure-text parser is unit-testable on any host.
public static class LinuxIpRouteParser
{
    /// Returns the gateway IP from a `default via X.X.X.X dev …` line, or
    /// <c>null</c> when the input is empty / malformed / missing the `via`
    /// token.
    public static string? ParseDefaultGateway(string ipRouteOutput)
    {
        if (string.IsNullOrWhiteSpace(ipRouteOutput)) return null;

        foreach (var line in ipRouteOutput.Split('\n'))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith("default ", StringComparison.Ordinal)) continue;

            var tokens = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < tokens.Length - 1; i++)
            {
                if (string.Equals(tokens[i], "via", StringComparison.Ordinal))
                {
                    var candidate = tokens[i + 1];
                    if (System.Net.IPAddress.TryParse(candidate, out _))
                    {
                        return candidate;
                    }
                }
            }
        }
        return null;
    }
}
