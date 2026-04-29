using AipmRegister.Core.Wifi;
using Microsoft.Extensions.Logging;

namespace AipmRegister.Hosting;

/// Placeholder Wi-Fi adapter that logs intent but performs no OS calls. Used
/// as the fallback when the build target is a platform we don't ship a
/// native adapter for, or when the runtime OS does not match the build's
/// platform define (which lets the orchestrator's Wi-Fi calls become
/// deterministic no-ops in those edge cases — useful for diagnostics).
internal sealed class NoopWifiAdapter : IWifiAdapter
{
    private readonly ILogger<NoopWifiAdapter> _logger;

    public NoopWifiAdapter(ILogger<NoopWifiAdapter> logger) => _logger = logger;

    public Task<IReadOnlyList<WifiNetwork>> ScanAsync(CancellationToken ct = default)
    {
        _logger.LogWarning("Wi-Fi scan requested but no adapter installed for this platform.");
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
