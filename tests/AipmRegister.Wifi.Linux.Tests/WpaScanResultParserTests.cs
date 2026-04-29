using AipmRegister.Core.Wifi;
using AipmRegister.Wifi.Linux;

namespace AipmRegister.Wifi.Linux.Tests;

public sealed class WpaScanResultParserTests
{
    private const string SampleScanOutput =
        "bssid / frequency / signal level / flags / ssid\n" +
        "aa:bb:cc:00:11:22\t2412\t-45\t[WPA2-PSK-CCMP][ESS]\tHomeWifi24\n" +
        "aa:bb:cc:00:11:33\t5180\t-60\t[WPA2-PSK-CCMP+TKIP][WPA-PSK-CCMP+TKIP][ESS]\tHomeWifi5G\n" +
        "aa:bb:cc:00:11:44\t2437\t-72\t[ESS]\tCafeOpen\n" +
        "aa:bb:cc:00:11:55\t2462\t-50\t[WEP][ESS]\tLegacy\n" +
        "aa:bb:cc:00:11:66\t5745\t-55\t[SAE][WPA2-PSK-CCMP][ESS]\tWpa3Hybrid\n" +
        "aa:bb:cc:00:11:77\t6105\t-58\t[WPA2-PSK-CCMP][ESS]\tWifi6E\n";

    [Fact]
    public void Parses_All_Rows_And_Skips_Header()
    {
        var result = WpaScanResultParser.Parse(SampleScanOutput);
        Assert.Equal(6, result.Count);
        Assert.Equal("HomeWifi24", result[0].Ssid);
        Assert.Equal("Wifi6E",     result[5].Ssid);
    }

    [Fact]
    public void Maps_Frequency_To_Band()
    {
        var result = WpaScanResultParser.Parse(SampleScanOutput);
        Assert.Equal("2.4G", result[0].Band);   // 2412 MHz
        Assert.Equal("5G",   result[1].Band);   // 5180 MHz
        Assert.Equal("2.4G", result[2].Band);
        Assert.Equal("2.4G", result[3].Band);
        Assert.Equal("5G",   result[4].Band);   // 5745 MHz
        Assert.Equal("6G",   result[5].Band);   // 6105 MHz
    }

    [Theory]
    [InlineData("[WPA2-PSK-CCMP][ESS]",                                WifiSecurity.Wpa2Personal)]
    [InlineData("[WPA-PSK-CCMP+TKIP][ESS]",                            WifiSecurity.WpaPersonal)]
    [InlineData("[ESS]",                                               WifiSecurity.Open)]
    [InlineData("[WEP][ESS]",                                          WifiSecurity.Wep)]
    [InlineData("[SAE][WPA2-PSK-CCMP][ESS]",                           WifiSecurity.Wpa3Personal)]
    [InlineData("[WPA3-PSK-CCMP][ESS]",                                WifiSecurity.Wpa3Personal)]
    public void Classifies_Security(string flags, WifiSecurity expected)
    {
        Assert.Equal(expected, WpaScanResultParser.ClassifySecurity(flags));
    }

    [Theory]
    [InlineData(-50,  100)]
    [InlineData(-30,  100)] // clamps high
    [InlineData(-100, 0)]
    [InlineData(-110, 0)]   // clamps low
    [InlineData(-75,  50)]
    [InlineData(-80,  40)]
    public void Clamps_Signal_To_0_100(int dbm, int expectedQuality)
    {
        Assert.Equal(expectedQuality, WpaScanResultParser.ClampSignalToQuality(dbm));
    }

    [Fact]
    public void Skips_Rows_With_Empty_Ssid()
    {
        var input =
            "bssid / frequency / signal level / flags / ssid\n" +
            "aa:bb:cc:00:11:22\t2412\t-45\t[WPA2-PSK-CCMP][ESS]\t\n" +    // empty SSID
            "aa:bb:cc:00:11:33\t2437\t-50\t[ESS]\tNamed\n";

        var result = WpaScanResultParser.Parse(input);
        Assert.Single(result);
        Assert.Equal("Named", result[0].Ssid);
    }

    [Fact]
    public void Recommended_Flag_Matches_Band_Plus_Security()
    {
        var result = WpaScanResultParser.Parse(SampleScanOutput);
        Assert.True(result[0].IsRecommended);   // 2.4 + WPA2 → green
        Assert.False(result[1].IsRecommended);  // 5G          → orange
        Assert.False(result[2].IsRecommended);  // open        → orange (no security)
        Assert.True(result[3].IsRecommended);   // 2.4 + WEP   → green (matches frmMain)
        Assert.False(result[4].IsRecommended);  // 5G WPA3     → orange
        Assert.False(result[5].IsRecommended);  // 6G          → orange
    }
}
