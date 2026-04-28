using AipmRegister.Core.Wifi;
using Microsoft.Extensions.Logging;

namespace AipmRegister.Gui.Wifi;

/// Same placeholder used by the CLI, kept identical so the GUI can run on
/// non-Windows platforms (and the orchestrator's Wi-Fi calls become no-ops).
internal sealed class NoopWifiAdapter : IWifiAdapter
{
    private readonly ILogger<NoopWifiAdapter> _logger;
    public NoopWifiAdapter(ILogger<NoopWifiAdapter> logger) => _logger = logger;

    public Task<IReadOnlyList<WifiNetwork>> ScanAsync(CancellationToken ct = default)
    {
        _logger.LogWarning("Wi-Fi scan requested but no adapter installed.");
        return Task.FromResult<IReadOnlyList<WifiNetwork>>(Array.Empty<WifiNetwork>());
    }

    public Task ConnectAsync(string ssid, string password, WifiSecurity security, CancellationToken ct = default)
    {
        _logger.LogWarning("Skipping connect to SSID={Ssid} — assuming user is already on the right network.", ssid);
        return Task.CompletedTask;
    }

    public Task DisconnectAndForgetAsync(string ssid, CancellationToken ct = default)
    {
        _logger.LogDebug("Skipping disconnect from SSID={Ssid}.", ssid);
        return Task.CompletedTask;
    }
}
