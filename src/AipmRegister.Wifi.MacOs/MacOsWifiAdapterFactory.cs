using System.Collections.Concurrent;
using System.Runtime.Versioning;
using AipmRegister.Core.Process;
using AipmRegister.Core.Wifi;
using Microsoft.Extensions.Logging;

namespace AipmRegister.Wifi.MacOs;

/// Factory that returns a per-interface <see cref="MacOsWifiAdapter"/>.
/// Caches by <see cref="WifiInterface.Id"/> so repeated resolutions for
/// the same chosen interface share one instance.
[SupportedOSPlatform("macos")]
public sealed class MacOsWifiAdapterFactory : IWifiAdapterFactory
{
    private readonly IProcessRunner _processRunner;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ConcurrentDictionary<string, IWifiAdapter> _cache = new(StringComparer.Ordinal);

    public MacOsWifiAdapterFactory(IProcessRunner processRunner, ILoggerFactory loggerFactory)
    {
        _processRunner = processRunner;
        _loggerFactory = loggerFactory;
    }

    public IWifiAdapter Create(WifiInterface iface) =>
        _cache.GetOrAdd(iface.Id, _ =>
            new MacOsWifiAdapter(iface, _processRunner, _loggerFactory.CreateLogger<MacOsWifiAdapter>()));
}
