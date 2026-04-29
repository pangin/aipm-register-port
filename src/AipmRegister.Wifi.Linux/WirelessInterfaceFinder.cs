using System.Diagnostics;
using System.Runtime.Versioning;

namespace AipmRegister.Wifi.Linux;

[SupportedOSPlatform("linux")]
internal static class WirelessInterfaceFinder
{
    private const string OverrideEnv = "AIPM_WIFI_IFACE";

    /// Finds the first wireless interface on the box. Order:
    ///   1. AIPM_WIFI_IFACE env var (explicit override)
    ///   2. /sys/class/net/&lt;iface&gt;/wireless directory
    ///   3. `iw dev` output, parsing the first "Interface" line
    public static string Find()
    {
        var fromEnv = Environment.GetEnvironmentVariable(OverrideEnv);
        if (!string.IsNullOrEmpty(fromEnv)) return fromEnv;

        const string sysClassNet = "/sys/class/net";
        if (Directory.Exists(sysClassNet))
        {
            foreach (var ifaceDir in Directory.EnumerateDirectories(sysClassNet))
            {
                if (Directory.Exists(Path.Combine(ifaceDir, "wireless")))
                {
                    return Path.GetFileName(ifaceDir);
                }
            }
        }

        // Fallback: parse `iw dev`
        var iw = TryRunIwDev();
        if (iw is not null) return iw;

        throw new InvalidOperationException(
            $"No wireless interface found. Set {OverrideEnv} explicitly or check `iw dev`.");
    }

    private static string? TryRunIwDev()
    {
        try
        {
            var psi = new ProcessStartInfo("iw", "dev")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };
            using var proc = Process.Start(psi);
            if (proc is null) return null;
            var output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit(2000);

            foreach (var line in output.Split('\n'))
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("Interface ", StringComparison.Ordinal))
                {
                    return trimmed.Substring("Interface ".Length).Trim();
                }
            }
        }
        catch
        {
            // iw might not be installed — fall through to the InvalidOperationException above.
        }
        return null;
    }
}
