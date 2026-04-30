using AipmRegister.Core.Wifi;

namespace AipmRegister.Core.Tests;

public sealed class WifiBandClassifierTests
{
    [Theory]
    [InlineData(2412, "2.4G")] // ch 1
    [InlineData(2484, "2.4G")] // ch 14 (Japan)
    [InlineData(5180, "5G")]   // ch 36
    [InlineData(5825, "5G")]   // ch 165
    [InlineData(6195, "6G")]   // ch 33 in 6 GHz numbering
    [InlineData(7115, "6G")]   // top of 6 GHz
    [InlineData(0,    "?")]
    [InlineData(1234, "?")]    // gap between 2.5 and 5 GHz
    [InlineData(4500, "?")]
    public void FromFrequencyMhz_MapsCanonicalBands(int mhz, string expected)
        => Assert.Equal(expected, WifiBandClassifier.FromFrequencyMhz(mhz));

    [Theory]
    [InlineData("6 (2.4 GHz, 20 MHz)", "2.4G")]
    [InlineData("6 (2GHz, 20MHz)",     "2.4G")]
    [InlineData("36 (5 GHz, 80 MHz)",  "5G")]
    [InlineData("161 (5GHz, 80MHz)",   "5G")]
    [InlineData("5 (6 GHz, 80 MHz)",   "6G")]
    [InlineData("5 (6GHz, 160MHz)",    "6G")]
    [InlineData("48",                  "5G")]
    [InlineData("11",                  "2.4G")]
    [InlineData("",                    "?")]
    [InlineData(null,                  "?")]
    [InlineData("nope",                "?")]
    public void FromChannel_ParsesSuffixOrNumeric(string? raw, string expected)
        => Assert.Equal(expected, WifiBandClassifier.FromChannel(raw));
}
