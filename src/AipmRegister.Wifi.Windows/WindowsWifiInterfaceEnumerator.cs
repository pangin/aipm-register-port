using System.Runtime.Versioning;
using AipmRegister.Core.Wifi;
using ManagedNativeWifi;

namespace AipmRegister.Wifi.Windows;

/// Lists wireless interfaces on Windows by calling
/// <c>ManagedNativeWifi.NativeWifi.EnumerateInterfaces()</c> (which wraps
/// <c>WlanEnumInterfaces</c> from wlanapi.dll). The driver description
/// becomes the user-facing label; the GUID stringified is the stable id
/// the rest of the pipeline passes back.
[SupportedOSPlatform("windows")]
public sealed class WindowsWifiInterfaceEnumerator : IWifiInterfaceEnumerator
{
    public Task<IReadOnlyList<WifiInterface>> EnumerateAsync(CancellationToken ct = default)
    {
        var result = new List<WifiInterface>();
        foreach (var iface in NativeWifi.EnumerateInterfaces())
        {
            ct.ThrowIfCancellationRequested();
            var id = iface.Id.ToString("D");
            var description = string.IsNullOrWhiteSpace(iface.Description)
                ? "Wi-Fi"
                : iface.Description;
            result.Add(new WifiInterface(Id: id, DisplayName: description));
        }
        return Task.FromResult<IReadOnlyList<WifiInterface>>(result);
    }
}
