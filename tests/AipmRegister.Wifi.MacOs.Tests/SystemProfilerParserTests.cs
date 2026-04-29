using AipmRegister.Core.Wifi;
using AipmRegister.Wifi.MacOs;

namespace AipmRegister.Wifi.MacOs.Tests;

public sealed class SystemProfilerParserTests
{
    /// Minimal but realistic plist matching `system_profiler SPAirPortDataType -xml`
    /// shape (one interface entry with three nearby APs).
    private const string SamplePlist = """
<?xml version="1.0" encoding="UTF-8"?>
<plist version="1.0">
<array>
  <dict>
    <key>_items</key>
    <array>
      <dict>
        <key>spairport_airport_other_local_wireless_networks</key>
        <array>
          <dict>
            <key>_name</key><string>HomeWifi24</string>
            <key>spairport_network_channel</key><string>6 (2.4 GHz, 20 MHz)</string>
            <key>spairport_security_mode</key><string>spairport_security_mode_wpa2_personal</string>
            <key>spairport_signal_noise</key><string>-45 dBm / -90 dBm</string>
          </dict>
          <dict>
            <key>_name</key><string>HomeWifi5G</string>
            <key>spairport_network_channel</key><string>36 (5 GHz, 80 MHz)</string>
            <key>spairport_security_mode</key><string>spairport_security_mode_wpa2_personal</string>
            <key>spairport_signal_noise</key><string>-60 dBm / -90 dBm</string>
          </dict>
          <dict>
            <key>_name</key><string>CafeOpen</string>
            <key>spairport_network_channel</key><string>11 (2.4 GHz, 20 MHz)</string>
            <key>spairport_security_mode</key><string>spairport_security_mode_none</string>
            <key>spairport_signal_noise</key><string>-72 dBm / -90 dBm</string>
          </dict>
        </array>
      </dict>
    </array>
  </dict>
</array>
</plist>
""";

    [Fact]
    public void Parses_All_Three_Networks()
    {
        var result = SystemProfilerParser.Parse(SamplePlist);
        Assert.Equal(3, result.Count);
        Assert.Equal("HomeWifi24", result[0].Ssid);
        Assert.Equal("HomeWifi5G", result[1].Ssid);
        Assert.Equal("CafeOpen",   result[2].Ssid);
    }

    [Fact]
    public void Maps_Channel_To_Band()
    {
        var result = SystemProfilerParser.Parse(SamplePlist);
        Assert.Equal("2.4G", result[0].Band);
        Assert.Equal("5G",   result[1].Band);
        Assert.Equal("2.4G", result[2].Band);
    }

    [Theory]
    [InlineData("spairport_security_mode_wpa3_personal", WifiSecurity.Wpa3Personal)]
    [InlineData("spairport_security_mode_wpa2_personal", WifiSecurity.Wpa2Personal)]
    [InlineData("spairport_security_mode_wpa_personal",  WifiSecurity.WpaPersonal)]
    [InlineData("spairport_security_mode_wep",           WifiSecurity.Wep)]
    [InlineData("spairport_security_mode_none",          WifiSecurity.Open)]
    [InlineData("",                                      WifiSecurity.Open)]
    public void Maps_Security_Mode(string raw, WifiSecurity expected)
    {
        Assert.Equal(expected, SystemProfilerParser.MapSecurity(raw));
    }

    [Theory]
    [InlineData("-45 dBm / -90 dBm", 100)]
    [InlineData("-75 dBm / -90 dBm", 50)]
    [InlineData("-100 dBm / -100 dBm", 0)]
    [InlineData("",                  0)]
    public void Parses_Signal_Quality(string raw, int expected)
    {
        Assert.Equal(expected, SystemProfilerParser.SignalNoiseToQuality(raw));
    }

    [Theory]
    [InlineData("6 (2.4 GHz, 20 MHz)", "2.4G")]
    [InlineData("36 (5 GHz, 80 MHz)",  "5G")]
    [InlineData("48 (5 GHz, 40 MHz)",  "5G")]
    [InlineData("5 (6 GHz, 80 MHz)",   "6G")]
    [InlineData("",                    "?")]
    public void Channel_Suffix_Beats_Numeric_For_6GHz(string raw, string expected)
    {
        Assert.Equal(expected, SystemProfilerParser.ChannelToBand(raw));
    }

    [Fact]
    public void Recommended_Flag_Matches_Band_Plus_Security()
    {
        var result = SystemProfilerParser.Parse(SamplePlist);
        Assert.True(result[0].IsRecommended);   // 2.4G + WPA2 → green
        Assert.False(result[1].IsRecommended);  // 5G + WPA2   → orange
        Assert.False(result[2].IsRecommended);  // 2.4G + open → orange
    }
}
