using System.Collections.Concurrent;
using System.Runtime.Versioning;
using AipmRegister.Core.Process;
using AipmRegister.Core.Wifi;
using Microsoft.Extensions.Logging;

namespace AipmRegister.Wifi.Linux;

/// Factory that returns a per-interface <see cref="LinuxWifiAdapter"/>.
/// Caches the instance keyed by <see cref="WifiInterface.Id"/> so repeated
/// resolutions for the same interface (e.g. WifiPicker → DevicePicker →
/// Registering) share one logger context and one "Using wireless
/// interface" log line.
[SupportedOSPlatform("linux")]
public sealed class LinuxWifiAdapterFactory : IWifiAdapterFactory
{
    private readonly IProcessRunner _processRunner;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ConcurrentDictionary<string, IWifiAdapter> _cache = new(StringComparer.Ordinal);

    public LinuxWifiAdapterFactory(IProcessRunner processRunner, ILoggerFactory loggerFactory)
    {
        _processRunner = processRunner;
        _loggerFactory = loggerFactory;
    }

    public IWifiAdapter Create(WifiInterface iface) =>
        _cache.GetOrAdd(iface.Id, _ =>
            new LinuxWifiAdapter(iface, _processRunner, _loggerFactory.CreateLogger<LinuxWifiAdapter>()));
}
