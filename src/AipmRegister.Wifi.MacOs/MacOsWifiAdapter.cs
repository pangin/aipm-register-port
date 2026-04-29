using System.Runtime.Versioning;
using AipmRegister.Core.Wifi;
using Microsoft.Extensions.Logging;

namespace AipmRegister.Wifi.MacOs;

/// IWifiAdapter implementation that drives macOS via the bundled
/// `networksetup` CLI plus `system_profiler` for scanning. Bound to a
/// specific Wi-Fi interface (e.g. "en0") chosen by the caller via
/// <see cref="MacOsWifiAdapterFactory"/>.
[SupportedOSPlatform("macos")]
public sealed class MacOsWifiAdapter : IWifiAdapter
{
    private readonly ILogger<MacOsWifiAdapter> _logger;
    private readonly string _interfaceName;

    public MacOsWifiAdapter(WifiInterface iface, ILogger<MacOsWifiAdapter> logger)
    {
        _logger = logger;
        _interfaceName = iface.Id;
        _logger.LogInformation("Using Wi-Fi interface: {Iface}", _interfaceName);
    }

    public async Task<IReadOnlyList<WifiNetwork>> ScanAsync(CancellationToken ct = default)
    {
        var raw = await NetworksetupRunner.RunSystemProfilerAsync(ct);
        return SystemProfilerParser.Parse(raw);
    }

    public async Task ConnectAsync(string ssid, string password, WifiSecurity security, CancellationToken ct = default)
    {
        var args = security == WifiSecurity.Open
            ? new[] { "-setairportnetwork", _interfaceName, ssid }
            : new[] { "-setairportnetwork", _interfaceName, ssid, password };

        var output = await NetworksetupRunner.RunAsync(args, ct);
        if (output.Contains("Could not", StringComparison.OrdinalIgnoreCase)
            || output.Contains("Failed",  StringComparison.OrdinalIgnoreCase)
            || output.Contains("error",   StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"networksetup refused to associate with '{ssid}': {output.Trim()}");
        }
        _logger.LogInformation("Connected to SSID={Ssid}", ssid);
    }

    public async Task DisconnectAndForgetAsync(string ssid, CancellationToken ct = default)
    {
        try
        {
            await NetworksetupRunner.RunAsync(
                new[] { "-removepreferredwirelessnetwork", _interfaceName, ssid }, ct);
            _logger.LogInformation("Removed preferred network for SSID={Ssid}", ssid);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Best-effort removal of SSID={Ssid} failed.", ssid);
        }
    }
}
