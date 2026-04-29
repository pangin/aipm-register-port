using System.Collections.Concurrent;
using System.Runtime.Versioning;
using AipmRegister.Core.Wifi;
using Microsoft.Extensions.Logging;

namespace AipmRegister.Wifi.MacOs;

/// Factory that returns a per-interface <see cref="MacOsWifiAdapter"/>.
/// Caches by <see cref="WifiInterface.Id"/> so repeated resolutions for
/// the same chosen interface share one instance.
[SupportedOSPlatform("macos")]
public sealed class MacOsWifiAdapterFactory : IWifiAdapterFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ConcurrentDictionary<string, IWifiAdapter> _cache = new(StringComparer.Ordinal);

    public MacOsWifiAdapterFactory(ILoggerFactory loggerFactory) => _loggerFactory = loggerFactory;

    public IWifiAdapter Create(WifiInterface iface) =>
        _cache.GetOrAdd(iface.Id, _ =>
            new MacOsWifiAdapter(iface, _loggerFactory.CreateLogger<MacOsWifiAdapter>()));
}
