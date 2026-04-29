using System.Runtime.Versioning;
using AipmRegister.Core.Wifi;
using Microsoft.Extensions.Logging;

namespace AipmRegister.Wifi.MacOs;

/// IWifiAdapter implementation that drives macOS via the bundled
/// `networksetup` CLI plus `system_profiler` for scanning.
[SupportedOSPlatform("macos")]
public sealed class MacOsWifiAdapter : IWifiAdapter
{
    private readonly ILogger<MacOsWifiAdapter> _logger;
    private string? _interfaceName;

    public MacOsWifiAdapter(ILogger<MacOsWifiAdapter> logger) => _logger = logger;

    private async Task<string> GetInterfaceAsync(CancellationToken ct)
    {
        if (_interfaceName is not null) return _interfaceName;

        var fromEnv = Environment.GetEnvironmentVariable("AIPM_WIFI_IFACE");
        if (!string.IsNullOrEmpty(fromEnv))
        {
            _interfaceName = fromEnv;
            return _interfaceName;
        }

        var raw = await NetworksetupRunner.RunAsync(new[] { "-listallhardwareports" }, ct);
        _interfaceName = NetworksetupRunner.FindWifiInterfaceName(raw);
        _logger.LogInformation("Using Wi-Fi interface: {Iface}", _interfaceName);
        return _interfaceName;
    }

    public async Task<IReadOnlyList<WifiNetwork>> ScanAsync(CancellationToken ct = default)
    {
        var raw = await NetworksetupRunner.RunSystemProfilerAsync(ct);
        return SystemProfilerParser.Parse(raw);
    }

    public async Task ConnectAsync(string ssid, string password, WifiSecurity security, CancellationToken ct = default)
    {
        var iface = await GetInterfaceAsync(ct);
        var args = security == WifiSecurity.Open
            ? new[] { "-setairportnetwork", iface, ssid }
            : new[] { "-setairportnetwork", iface, ssid, password };

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
            var iface = await GetInterfaceAsync(ct);
            await NetworksetupRunner.RunAsync(new[] { "-removepreferredwirelessnetwork", iface, ssid }, ct);
            _logger.LogInformation("Removed preferred network for SSID={Ssid}", ssid);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Best-effort removal of SSID={Ssid} failed.", ssid);
        }
    }
}
