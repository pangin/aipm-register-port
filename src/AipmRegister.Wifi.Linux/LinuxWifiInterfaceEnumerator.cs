using System.Runtime.Versioning;
using AipmRegister.Core.Process;
using AipmRegister.Core.Wifi;

namespace AipmRegister.Wifi.Linux;

/// Lists wireless interfaces on Linux. Two complementary sources, in
/// order:
///   1. `/sys/class/net/&lt;iface&gt;/wireless` directories — present whenever
///      the kernel exposes a cfg80211/nl80211 device. Cheap and works in
///      stripped-down containers without `iw` installed.
///   2. `iw dev` — used as a supplement when sysfs returned nothing
///      (rare, but harmless to try).
///
/// Returned <see cref="WifiInterface.Id"/> is the kernel netdev name
/// (wlan0, wlp3s0, …); <see cref="WifiInterface.DisplayName"/> is the
/// same string (Linux doesn't expose a friendly name without nmcli /
/// udevadm, which we deliberately don't depend on).
[SupportedOSPlatform("linux")]
public sealed class LinuxWifiInterfaceEnumerator : IWifiInterfaceEnumerator
{
    private readonly IProcessRunner _processRunner;

    public LinuxWifiInterfaceEnumerator(IProcessRunner processRunner) => _processRunner = processRunner;

    public async Task<IReadOnlyList<WifiInterface>> EnumerateAsync(CancellationToken ct = default)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<WifiInterface>();

        const string sysClassNet = "/sys/class/net";
        if (Directory.Exists(sysClassNet))
        {
            foreach (var ifaceDir in Directory.EnumerateDirectories(sysClassNet))
            {
                ct.ThrowIfCancellationRequested();
                if (Directory.Exists(Path.Combine(ifaceDir, "wireless")))
                {
                    var name = Path.GetFileName(ifaceDir);
                    if (seen.Add(name))
                    {
                        result.Add(new WifiInterface(Id: name, DisplayName: name));
                    }
                }
            }
        }

        if (result.Count == 0)
        {
            foreach (var name in await TryRunIwDevAsync(ct))
            {
                if (seen.Add(name))
                {
                    result.Add(new WifiInterface(Id: name, DisplayName: name));
                }
            }
        }

        return result;
    }

    private async Task<IReadOnlyList<string>> TryRunIwDevAsync(CancellationToken ct)
    {
        try
        {
            var result = await _processRunner.RunAsync("iw", new[] { "dev" }, ct);
            return WirelessInterfaceParser.ParseIwDev(result.Stdout);
        }
        catch
        {
            // iw might not be installed — sysfs is the primary path anyway.
            return Array.Empty<string>();
        }
    }
}
