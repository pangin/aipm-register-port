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

    /// Actual modern macOS shape: `_items` contains a dictionary with
    /// `spairport_airport_interfaces`, and each interface dictionary owns the
    /// current/nearby network arrays.
    private const string NestedInterfacePlist = """
<?xml version="1.0" encoding="UTF-8"?>
<plist version="1.0">
<array>
  <dict>
    <key>_items</key>
    <array>
      <dict>
        <key>spairport_airport_interfaces</key>
        <array>
          <dict>
            <key>_name</key><string>en0</string>
            <key>spairport_current_network_information</key>
            <dict>
              <key>_name</key><string>Current6G</string>
              <key>spairport_network_channel</key><string>5 (6GHz, 160MHz)</string>
              <key>spairport_security_mode</key><string>spairport_security_mode_wpa3_personal</string>
              <key>spairport_signal_noise</key><string>-76 dBm / -92 dBm</string>
            </dict>
            <key>spairport_airport_other_local_wireless_networks</key>
            <array>
              <dict>
                <key>_name</key><string>Other24</string>
                <key>spairport_network_channel</key><string>6 (2GHz, 20MHz)</string>
                <key>spairport_security_mode</key><string>spairport_security_mode_wpa2_personal</string>
              </dict>
            </array>
          </dict>
          <dict>
            <key>_name</key><string>en7</string>
            <key>spairport_airport_other_local_wireless_networks</key>
            <array>
              <dict>
                <key>_name</key><string>UsbWifi5G</string>
                <key>spairport_network_channel</key><string>161 (5GHz, 80MHz)</string>
                <key>spairport_security_mode</key><string>spairport_security_mode_wpa2_personal_mixed</string>
              </dict>
            </array>
          </dict>
          <dict>
            <key>_name</key><string>awdl0</string>
            <key>spairport_current_network_information</key>
            <dict>
              <key>spairport_network_type</key><string>spairport_network_type_station</string>
            </dict>
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
    public void Parses_Modern_Nested_Interface_Shape()
    {
        var result = SystemProfilerParser.Parse(NestedInterfacePlist, "en0");

        Assert.Equal(2, result.Count);
        Assert.Equal("Current6G", result[0].Ssid);
        Assert.Equal("6G", result[0].Band);
        Assert.Equal(48, result[0].SignalQuality);
        Assert.Equal(WifiSecurity.Wpa3Personal, result[0].Security);

        Assert.Equal("Other24", result[1].Ssid);
        Assert.Equal("2.4G", result[1].Band);
        Assert.Equal(0, result[1].SignalQuality);
        Assert.Equal(WifiSecurity.Wpa2Personal, result[1].Security);
    }

    [Fact]
    public void Filters_To_Selected_Interface()
    {
        var result = SystemProfilerParser.Parse(NestedInterfacePlist, "en7");

        Assert.Single(result);
        Assert.Equal("UsbWifi5G", result[0].Ssid);
        Assert.Equal("5G", result[0].Band);
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
    [InlineData("6 (2GHz, 20MHz)",      "2.4G")]
    [InlineData("36 (5 GHz, 80 MHz)",  "5G")]
    [InlineData("161 (5GHz, 80MHz)",    "5G")]
    [InlineData("48 (5 GHz, 40 MHz)",  "5G")]
    [InlineData("5 (6 GHz, 80 MHz)",   "6G")]
    [InlineData("5 (6GHz, 160MHz)",     "6G")]
    [InlineData("",                    "?")]
    public void Channel_Suffix_Beats_Numeric_For_6GHz(string raw, string expected)
    {
        Assert.Equal(expected, AipmRegister.Core.Wifi.WifiBandClassifier.FromChannel(raw));
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
