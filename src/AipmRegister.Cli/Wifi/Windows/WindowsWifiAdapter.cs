using System.Runtime.Versioning;
using AipmRegister.Core.Wifi;
using ManagedNativeWifi;
using Microsoft.Extensions.Logging;

namespace AipmRegister.Wifi.Windows;

/// Windows-only IWifiAdapter implementation backed by ManagedNativeWifi
/// (which wraps wlanapi.dll). Replaces the inline P/Invoke calls scattered
/// through frmMain (lines 1140-1260, 1761) of the original.
[SupportedOSPlatform("windows")]
internal sealed class WindowsWifiAdapter : IWifiAdapter
{
    private readonly ILogger<WindowsWifiAdapter> _logger;
    private readonly TimeSpan _connectTimeout = TimeSpan.FromSeconds(20);

    public WindowsWifiAdapter(ILogger<WindowsWifiAdapter> logger) => _logger = logger;

    public Task<IReadOnlyList<WifiNetwork>> ScanAsync(CancellationToken ct = default)
    {
        var result = new List<WifiNetwork>();
        foreach (var bss in NativeWifi.EnumerateBssNetworks())
        {
            ct.ThrowIfCancellationRequested();
            var ssid = bss.Ssid.ToString();
            if (string.IsNullOrWhiteSpace(ssid)) continue;

            var band = bss.Frequency switch
            {
                >= 5000 and < 6000 => "5G",
                >= 2400 and < 2500 => "2.4G",
                >= 6000            => "6G",
                _                  => "?",
            };

            // BssNetworkPack does not expose authentication info; we cross
            // it with AvailableNetworkPack lookups in MapSecurity.
            result.Add(new WifiNetwork(
                Ssid:          ssid,
                SignalQuality: (int)bss.LinkQuality,
                Security:      LookupSecurity(ssid),
                Band:          band));
        }
        return Task.FromResult<IReadOnlyList<WifiNetwork>>(result);
    }

    public async Task ConnectAsync(string ssid, string password, WifiSecurity security, CancellationToken ct = default)
    {
        var iface = NativeWifi.EnumerateInterfaces().FirstOrDefault()
            ?? throw new InvalidOperationException("No Wi-Fi interface found.");

        var profileXml = BuildWlanProfileXml(ssid, password, security);
        _logger.LogInformation("Installing WLAN profile for SSID={Ssid}", ssid);

        var installed = NativeWifi.SetProfile(
            interfaceId: iface.Id,
            profileType: ProfileType.AllUser,
            profileXml: profileXml,
            profileSecurity: null,
            overwrite: true);

        if (!installed)
        {
            throw new InvalidOperationException($"Failed to install WLAN profile for SSID '{ssid}'.");
        }

        var connected = await NativeWifi.ConnectNetworkAsync(
            interfaceId: iface.Id,
            profileName: ssid,
            bssType: BssType.Any,
            timeout: _connectTimeout,
            cancellationToken: ct);

        if (!connected)
        {
            throw new InvalidOperationException(
                $"Failed to associate with SSID '{ssid}' within {_connectTimeout.TotalSeconds:0}s.");
        }

        _logger.LogInformation("Connected to SSID={Ssid}", ssid);
    }

    public Task DisconnectAndForgetAsync(string ssid, CancellationToken ct = default)
    {
        try
        {
            foreach (var iface in NativeWifi.EnumerateInterfaces())
            {
                NativeWifi.DeleteProfile(iface.Id, ssid);
            }
            _logger.LogInformation("Removed WLAN profile for SSID={Ssid}", ssid);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Best-effort profile removal for SSID={Ssid} failed.", ssid);
        }
        return Task.CompletedTask;
    }

    /// Cross-checks the SSID against AvailableNetworkPack entries to pick a
    /// reasonable security default. Falls back to WPA2-Personal because the
    /// vast majority of consumer APs we'd encounter use it.
    private static WifiSecurity LookupSecurity(string ssid)
    {
        try
        {
            foreach (var avail in NativeWifi.EnumerateAvailableNetworks())
            {
                if (string.Equals(avail.Ssid.ToString(), ssid, StringComparison.Ordinal))
                {
                    if (avail.IsSecurityEnabled == false) return WifiSecurity.Open;
                    break;
                }
            }
        }
        catch
        {
            // If anything goes sideways we'd rather assume WPA2 than crash
            // the scan loop.
        }
        return WifiSecurity.Wpa2Personal;
    }

    private static string BuildWlanProfileXml(string ssid, string password, WifiSecurity security)
    {
        // Hex-encode the SSID, matching the format the original frmMain
        // produced at line 1562.
        var hex = string.Concat(System.Text.Encoding.UTF8.GetBytes(ssid)
            .Select(b => b.ToString("X2")));

        var (auth, encryption, hasKey) = security switch
        {
            WifiSecurity.Open          => ("open",    "none", false),
            WifiSecurity.Wep           => ("open",    "WEP",  true),
            WifiSecurity.WpaPersonal   => ("WPAPSK",  "TKIP", true),
            WifiSecurity.Wpa2Personal  => ("WPA2PSK", "AES",  true),
            WifiSecurity.Wpa3Personal  => ("WPA3SAE", "AES",  true),
            _                          => ("WPA2PSK", "AES",  true),
        };

        var sharedKey = hasKey ? $@"
      <sharedKey>
        <keyType>passPhrase</keyType>
        <protected>false</protected>
        <keyMaterial>{System.Security.SecurityElement.Escape(password)}</keyMaterial>
      </sharedKey>" : string.Empty;

        return $@"<?xml version=""1.0""?>
<WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
  <name>{System.Security.SecurityElement.Escape(ssid)}</name>
  <SSIDConfig>
    <SSID>
      <hex>{hex}</hex>
      <name>{System.Security.SecurityElement.Escape(ssid)}</name>
    </SSID>
  </SSIDConfig>
  <connectionType>ESS</connectionType>
  <connectionMode>manual</connectionMode>
  <autoSwitch>false</autoSwitch>
  <MSM>
    <security>
      <authEncryption>
        <authentication>{auth}</authentication>
        <encryption>{encryption}</encryption>
        <useOneX>false</useOneX>
      </authEncryption>{sharedKey}
    </security>
  </MSM>
</WLANProfile>";
    }
}
