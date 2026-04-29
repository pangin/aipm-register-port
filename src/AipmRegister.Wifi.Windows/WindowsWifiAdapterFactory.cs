using System.Collections.Concurrent;
using System.Runtime.Versioning;
using AipmRegister.Core.Wifi;
using Microsoft.Extensions.Logging;

namespace AipmRegister.Wifi.Windows;

/// Factory that returns a per-interface <see cref="WindowsWifiAdapter"/>.
/// Caches by <see cref="WifiInterface.Id"/> (the interface GUID) so
/// repeated resolutions for the same chosen adapter share one instance
/// and the "Using Wi-Fi interface" log line fires once.
[SupportedOSPlatform("windows")]
public sealed class WindowsWifiAdapterFactory : IWifiAdapterFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ConcurrentDictionary<string, IWifiAdapter> _cache = new(StringComparer.Ordinal);

    public WindowsWifiAdapterFactory(ILoggerFactory loggerFactory) => _loggerFactory = loggerFactory;

    public IWifiAdapter Create(WifiInterface iface) =>
        _cache.GetOrAdd(iface.Id, _ =>
            new WindowsWifiAdapter(iface, _loggerFactory.CreateLogger<WindowsWifiAdapter>()));
}
