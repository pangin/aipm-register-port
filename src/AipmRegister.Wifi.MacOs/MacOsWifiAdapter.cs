using System.Runtime.Versioning;
using AipmRegister.Core.Process;
using AipmRegister.Core.Wifi;
using Microsoft.Extensions.Logging;

namespace AipmRegister.Wifi.MacOs;

/// IWifiAdapter implementation that drives macOS via the bundled
/// `networksetup` CLI plus `system_profiler` for scanning. Bound to a
/// specific Wi-Fi interface (e.g. "en0") chosen by the caller via
/// <see cref="MacOsWifiAdapterFactory"/>.
[SupportedOSPlatform("macos")]
public sealed class MacOsWifiAdapter : IWifiAdapter, IWifiGatewayProvider
{
    private readonly IProcessRunner _processRunner;
    private readonly ILogger<MacOsWifiAdapter> _logger;
    private readonly string _interfaceName;

    public MacOsWifiAdapter(WifiInterface iface, IProcessRunner processRunner, ILogger<MacOsWifiAdapter> logger)
    {
        _processRunner = processRunner;
        _logger = logger;
        _interfaceName = iface.Id;
        _logger.LogInformation("Using Wi-Fi interface: {Iface}", _interfaceName);
    }

    public async Task<IReadOnlyList<WifiNetwork>> ScanAsync(CancellationToken ct = default)
    {
        MacOsLocationPermission.RequestWhenInUseAuthorizationForBundledApp(_logger);

        var networks = await Task.Run(() => CoreWlanScanner.Scan(_interfaceName), ct);

        _logger.LogInformation("Found {Count} Wi-Fi networks on interface {Iface}", networks.Count, _interfaceName);
        return networks;
    }

    public async Task ConnectAsync(string ssid, string password, WifiSecurity security, CancellationToken ct = default)
    {
        var args = security == WifiSecurity.Open
            ? new[] { "-setairportnetwork", _interfaceName, ssid }
            : new[] { "-setairportnetwork", _interfaceName, ssid, password };

        var output = await NetworksetupRunner.RunAsync(_processRunner, args, ct);
        if (output.Contains("Could not", StringComparison.OrdinalIgnoreCase)
            || output.Contains("Failed",  StringComparison.OrdinalIgnoreCase)
            || output.Contains("error",   StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"networksetup refused to associate with '{ssid}': {output.Trim()}");
        }

        var gateway = await GetGatewayAsync(TimeSpan.FromSeconds(20), ct);
        _logger.LogInformation("Connected to SSID={Ssid}; gateway={Gateway}", ssid, gateway ?? "-");
    }

    public async Task<string?> GetGatewayAsync(TimeSpan timeout, CancellationToken ct = default)
    {
        var deadline = DateTime.UtcNow + timeout;
        Exception? lastError = null;

        while (DateTime.UtcNow < deadline)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var result = await _processRunner.RunAsync("ipconfig", new[] { "getpacket", _interfaceName }, ct);
                if (result.ExitCode == 0)
                {
                    var router = MacOsDhcpLeaseParser.ParseRouter(result.Stdout);
                    if (!string.IsNullOrWhiteSpace(router)) return router;
                }
                else
                {
                    lastError = new InvalidOperationException(result.Stderr.Trim());
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                lastError = ex;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(500), ct);
        }

        if (lastError is not null)
        {
            _logger.LogWarning(lastError, "Could not read gateway for Wi-Fi interface {Iface}.", _interfaceName);
        }
        return null;
    }

    public async Task DisconnectAndForgetAsync(string ssid, CancellationToken ct = default)
    {
        try
        {
            await NetworksetupRunner.RunAsync(_processRunner,
                new[] { "-removepreferredwirelessnetwork", _interfaceName, ssid }, ct);
            _logger.LogInformation("Removed preferred network for SSID={Ssid}", ssid);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Best-effort removal of SSID={Ssid} failed.", ssid);
        }
    }
}
