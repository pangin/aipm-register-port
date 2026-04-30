using AipmRegister.Core.Models;

namespace AipmRegister.Core.Tests;

public sealed class HotspotSsidParserTests
{
    [Theory]
    [InlineData("DAWON_IRBD_AABBCC", "AABBCC")]
    [InlineData("DWD-S120_3b12b9",   "3b12b9")]
    [InlineData("DWD-PowerStrip-9F", "9F")]
    [InlineData("VendorTag",         "VendorTag")] // no separator → whole string
    public void ExtractMac_ReturnsTrailingTokenAfterUnderscoreOrDash(string ssid, string expected)
        => Assert.Equal(expected, HotspotSsidParser.ExtractMac(ssid));

    [Fact]
    public void ExtractMac_HandlesTrailingSeparator()
    {
        // A separator at the very end means there is no MAC tail; preserve
        // the whole SSID instead of returning empty (matches the
        // pre-extraction inline behavior).
        Assert.Equal("DWD-S120_", HotspotSsidParser.ExtractMac("DWD-S120_"));
    }

    [Fact]
    public void ResolveProduct_FromKnownSsid_ReturnsCatalogEntry()
    {
        var ssid = ProductCatalog.All[0].PrimaryPrefix + "_AABBCC";
        var picked = HotspotSsidParser.ResolveProduct(ssid);
        Assert.Equal(ProductCatalog.All[0].Tag, picked.Tag);
    }

    [Fact]
    public void ResolveProduct_FromUnknownSsid_FabricatesProductFromPrefix()
    {
        var picked = HotspotSsidParser.ResolveProduct("UNKNOWN_AABBCC");
        Assert.Equal("UNKNOWN", picked.PrimaryPrefix);
        Assert.Equal("UNKNOWN", picked.ModelCode);
    }

    [Theory]
    [InlineData("S120",  "DWD-S120_AABBCC",  "B530_W")]
    [InlineData("S120",  "DWD-LS120_AABBCC", "B540_W")]
    [InlineData("ES120", "DWD-ES120_AABBCC", "B550E_W")]
    [InlineData("ES120", "DWD-SS120_AABBCC", "B550_W")]
    [InlineData("S310",  "DWD-S311_AABBCC",  "P110_WA")]
    [InlineData("S510",  "DWD-S510_AABBCC",  "R110_W")]
    [InlineData("S501",  "DWD-S510_AABBCC",  "R200_W")]
    [InlineData("S600",  "DWD-S600_AABBCC",  "B400_SW")]
    [InlineData("S110",  "DWD-S600_AABBCC",  "B400_W")]
    public void ResolveModelCode_Mirrors_Legacy_Product_And_Ssid_Mapping(
        string pickedTag,
        string hotspotSsid,
        string expected)
    {
        var picked = ProductCatalog.All.Single(p => p.Tag == pickedTag);
        Assert.Equal(expected, ProductCatalog.ResolveModelCode(hotspotSsid, picked));
    }
}
