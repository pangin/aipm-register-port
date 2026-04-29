using System.Runtime.Versioning;
using AipmRegister.Core.Wifi;
using Microsoft.Extensions.Logging;

namespace AipmRegister.Wifi.Linux;

/// IWifiAdapter implementation that drives wpa_supplicant via its control
/// socket. Works on any Linux box where wpa_supplicant is the active
/// supplicant (most consumer distros, including those running NetworkManager,
/// since NM ships its own wpa_supplicant under the hood).
[SupportedOSPlatform("linux")]
public sealed class LinuxWifiAdapter : IWifiAdapter
{
    private readonly ILogger<LinuxWifiAdapter> _logger;
    private readonly string _interfaceName;

    public LinuxWifiAdapter(WifiInterface iface, ILogger<LinuxWifiAdapter> logger)
    {
        _logger = logger;
        _interfaceName = iface.Id;
        _logger.LogInformation("Using wireless interface: {Iface}", _interfaceName);
    }

    public async Task<IReadOnlyList<WifiNetwork>> ScanAsync(CancellationToken ct = default)
    {
        using var client = new WpaSupplicantClient(_interfaceName);
        // Trigger a fresh scan; wpa_supplicant returns 'OK' (or 'FAIL-BUSY' if
        // a scan is in progress, which we treat as fine).
        var scanReply = await client.ScanAsync(ct);
        if (!string.Equals(scanReply.Trim(), "OK", StringComparison.Ordinal)
            && !scanReply.Contains("BUSY", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("SCAN reply unexpected: {Reply}", scanReply);
        }

        // Give the radio a moment to settle. The original frmMain timer fired
        // every ~250ms; 800ms is conservative without being annoying.
        await Task.Delay(TimeSpan.FromMilliseconds(800), ct);

        var raw = await client.ScanResultsAsync(ct);
        return WpaScanResultParser.Parse(raw);
    }

    public async Task ConnectAsync(string ssid, string password, WifiSecurity security, CancellationToken ct = default)
    {
        using var client = new WpaSupplicantClient(_interfaceName);

        // Make sure we don't pile up duplicate profiles.
        await RemoveProfilesNamedAsync(client, ssid, ct);

        var id = await client.AddNetworkAsync(ct);
        await client.SetNetworkStringAsync(id, "ssid", ssid, ct);

        switch (security)
        {
            case WifiSecurity.Open:
                await client.SetNetworkRawAsync(id, "key_mgmt", "NONE", ct);
                break;

            case WifiSecurity.Wep:
                await client.SetNetworkRawAsync(id, "key_mgmt", "NONE", ct);
                await client.SetNetworkStringAsync(id, "wep_key0", password, ct);
                await client.SetNetworkRawAsync(id, "wep_tx_keyidx", "0", ct);
                break;

            case WifiSecurity.WpaPersonal:
            case WifiSecurity.Wpa2Personal:
                await client.SetNetworkRawAsync(id, "key_mgmt", "WPA-PSK", ct);
                await client.SetNetworkStringAsync(id, "psk", password, ct);
                break;

            case WifiSecurity.Wpa3Personal:
                await client.SetNetworkRawAsync(id, "key_mgmt", "SAE", ct);
                await client.SetNetworkStringAsync(id, "sae_password", password, ct);
                await client.SetNetworkRawAsync(id, "ieee80211w", "2", ct);
                break;
        }

        await client.EnableNetworkAsync(id, ct);
        await client.SelectNetworkAsync(id, ct);

        await WaitForCompletionAsync(client, ssid, TimeSpan.FromSeconds(20), ct);
        _logger.LogInformation("Connected to SSID={Ssid}", ssid);
    }

    public async Task DisconnectAndForgetAsync(string ssid, CancellationToken ct = default)
    {
        try
        {
            using var client = new WpaSupplicantClient(_interfaceName);
            await RemoveProfilesNamedAsync(client, ssid, ct);
            _logger.LogInformation("Removed wpa_supplicant profile for SSID={Ssid}", ssid);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Best-effort profile removal for SSID={Ssid} failed.", ssid);
        }
    }

    private static async Task RemoveProfilesNamedAsync(WpaSupplicantClient client, string ssid, CancellationToken ct)
    {
        var listing = await client.ListNetworksAsync(ct);
        // First line is a header. Each row: "<id>\t<ssid>\t<bssid>\t<flags>"
        var lines = listing.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (line.StartsWith("network id", StringComparison.OrdinalIgnoreCase)) continue;
            var parts = line.Split('\t');
            if (parts.Length < 2) continue;
            if (!int.TryParse(parts[0], out var id)) continue;
            if (string.Equals(parts[1], ssid, StringComparison.Ordinal))
            {
                await client.RemoveNetworkAsync(id, ct);
            }
        }
    }

    private async Task WaitForCompletionAsync(
        WpaSupplicantClient client, string ssid, TimeSpan timeout, CancellationToken ct)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            ct.ThrowIfCancellationRequested();
            var status = await client.StatusAsync(ct);
            if (status.Contains("wpa_state=COMPLETED", StringComparison.Ordinal)
                && status.Contains($"ssid={ssid}", StringComparison.Ordinal))
            {
                return;
            }
            await Task.Delay(TimeSpan.FromMilliseconds(500), ct);
        }
        throw new TimeoutException(
            $"Failed to associate with SSID '{ssid}' within {timeout.TotalSeconds:0}s.");
    }
}
