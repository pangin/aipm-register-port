using System.Runtime.Versioning;
using AipmRegister.Core.Wifi;

namespace AipmRegister.Wifi.MacOs;

/// Lists Wi-Fi network interfaces on macOS by parsing
/// `networksetup -listallhardwareports`. Each "Hardware Port: Wi-Fi"
/// block becomes a <see cref="WifiInterface"/>. Returned in the order
/// `networksetup` emits them (which mirrors System Settings).
[SupportedOSPlatform("macos")]
public sealed class MacOsWifiInterfaceEnumerator : IWifiInterfaceEnumerator
{
    public async Task<IReadOnlyList<WifiInterface>> EnumerateAsync(CancellationToken ct = default)
    {
        var raw = await NetworksetupRunner.RunAsync(new[] { "-listallhardwareports" }, ct);
        return HardwarePortsParser.Parse(raw);
    }
}
