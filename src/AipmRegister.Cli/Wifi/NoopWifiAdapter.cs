using AipmRegister.Core.Wifi;
using Microsoft.Extensions.Logging;

namespace AipmRegister.Cli.Wifi;

/// Placeholder Wi-Fi adapter that logs intent but performs no OS calls. Used
/// while the real Win32 NativeWifi adapter is being ported (next commit). Lets
/// the CLI exercise the orchestrator end-to-end against a network the user
/// has already connected to manually.
internal sealed class NoopWifiAdapter : IWifiAdapter
{
    private readonly ILogger<NoopWifiAdapter> _logger;

    public NoopWifiAdapter(ILogger<NoopWifiAdapter> logger) => _logger = logger;

    public Task<IReadOnlyList<WifiNetwork>> ScanAsync(CancellationToken ct = default)
    {
        _logger.LogWarning("Wi-Fi scan requested but no adapter installed (Win32 implementation pending).");
        return Task.FromResult<IReadOnlyList<WifiNetwork>>(Array.Empty<WifiNetwork>());
    }

    public Task ConnectAsync(string ssid, string password, WifiSecurity security, CancellationToken ct = default)
    {
        _logger.LogWarning(
            "Skipping connect to SSID={Ssid} (security={Security}) — assuming the user is already on the right network.",
            ssid, security);
        return Task.CompletedTask;
    }

    public Task DisconnectAndForgetAsync(string ssid, CancellationToken ct = default)
    {
        _logger.LogDebug("Skipping disconnect from SSID={Ssid}.", ssid);
        return Task.CompletedTask;
    }
}
