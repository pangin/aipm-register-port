namespace AipmRegister.Core.Models;

/// One DAWON IoT device SKU. Recovered from frmMain.cs designer code
/// (lines 652-681) and the c()/_3() handlers (lines 2010-2159).
public sealed record ProductDefinition(
    /// Internal tag the original used as the ListView item Tag — also the
    /// suffix of the device's primary hotspot SSID prefix.
    string Tag,
    /// Resource key into Strings.{ko,en}.resx. The Korean baseline matches
    /// the original ToolTipText word-for-word.
    string DisplayKey,
    /// Resource key into Assets/Icons.axaml (StreamGeometry x:Key).
    string IconKey,
    /// Primary hotspot SSID prefix (always "DWD-" + Tag).
    string PrimaryPrefix,
    /// Secondary prefix (some SKUs share two hotspot families).
    /// Empty when the SKU has only the primary.
    string SecondaryPrefix,
    /// Model code embedded in the device's TCP-5000 reply. The orchestrator
    /// uses this when the device responds with a primary-prefix SSID. The
    /// (rare) PrimaryAlt cases match the legacy frmMain ternary in _3().
    string ModelCode,
    /// Optional override for when the picked PrimaryPrefix is the alternate
    /// form. e.g. ES120 normally maps to B550E_W, but if the user actually
    /// chose ES120S the model becomes B550E_SW.
    string? ModelCodeWhenAltPrefix = null,
    /// Optional override that fires when the device responds with the
    /// secondary prefix instead of the primary one. e.g. picking S510 on the
    /// IR Remote SKU yields R110_W instead of R200_W.
    string? ModelCodeWhenSecondaryUsed = null);

public static class ProductCatalog
{
    /// All 15 SKUs in the order the original ListView shows them.
    public static IReadOnlyList<ProductDefinition> All { get; } = new[]
    {
        new ProductDefinition("S120",   "Product.SmartPlug16A.S120.Name",      "Icon.SmartPlug",       "DWD-S120",   "DWD-LS120", "B530_W"),
        new ProductDefinition("ES120",  "Product.SmartPlug16A.ES120.Name",     "Icon.SmartPlug",       "DWD-ES120",  "DWD-SS120", "B550E_W",
            ModelCodeWhenAltPrefix: "B550E_SW"), // when primary becomes DWD-ES120S
        new ProductDefinition("LS130",  "Product.SmartPlug16A.LS130.Name",     "Icon.SmartPlug",       "DWD-LS130",  "",          "B350_W"),
        new ProductDefinition("S220",   "Product.SmartMultitap16A.Name",       "Icon.MultiTap",        "DWD-S220",   "",          "M130_W"),
        new ProductDefinition("LS810",  "Product.ZigbeeHub.Name",              "Icon.ZigbeeHub",       "DWD-LS810",  "",          "G200L_ZB"),
        new ProductDefinition("S510",   "Product.IrRemote.S510.Name",          "Icon.IrRemote",        "DWD-S510",   "",          "R200_W",
            ModelCodeWhenAltPrefix: "R110_W"),
        new ProductDefinition("S501",   "Product.IrRemote.S501.Name",          "Icon.IrRemote",        "DWD-S501",   "DWD-S510",  "R200_W"),
        new ProductDefinition("S310",   "Product.PanelboardSingle50A.Name",    "Icon.Panelboard",      "DWD-S310",   "DWD-S311",  "P110_W"),
        new ProductDefinition("S330",   "Product.PanelboardThree100A.Name",    "Icon.Panelboard",      "DWD-S330",   "",          "P230_W"),
        new ProductDefinition("S350",   "Product.PanelboardThree400A.Name",    "Icon.Panelboard",      "DWD-S350",   "",          "P250_W"),
        new ProductDefinition("S370",   "Product.PanelboardThree800A.Name",    "Icon.Panelboard",      "DWD-S370",   "",          "P270_W"),
        new ProductDefinition("ES120S", "Product.SolarSmartPlug16A.Name",      "Icon.SolarPlug",       "DWD-ES120S", "DWD-ES120", "B550E_SW"),
        new ProductDefinition("S600",   "Product.SolarSmartPlug10A.Name",      "Icon.SolarPlug",       "DWD-S600",   "",          "B400_W",
            ModelCodeWhenAltPrefix: "B400_SW"),
        new ProductDefinition("S110",   "Product.SmartPlug10A.Name",           "Icon.SmartPlug",       "DWD-S110",   "DWD-S600",  "B400_WI"),
        new ProductDefinition("S121",   "Product.SmartPlugJP.Name",            "Icon.SmartPlug",       "DWD-S121",   "",          "B343_W"),
    };

    /// Returns true iff `ssid` could be a hotspot of `product` (i.e. starts
    /// with either of its prefixes).
    public static bool IsHotspotOf(this ProductDefinition product, string ssid)
        => ssid.StartsWith(product.PrimaryPrefix, StringComparison.Ordinal)
            || (!string.IsNullOrEmpty(product.SecondaryPrefix)
                && ssid.StartsWith(product.SecondaryPrefix, StringComparison.Ordinal));

    /// Resolve the model code recovered from a device-reported SSID such as
    /// "DWD-S120_3b12b9". Mirrors frmMain.cs _3() (line 2106) but uses the
    /// catalog instead of a hand-rolled switch.
    public static string ResolveModelCode(string deviceReportedSsid, ProductDefinition picked)
    {
        // Strip the "DWD-" prefix and pre-underscore tail: "DWD-S120_3b12b9" -> "S120".
        var token = deviceReportedSsid.Split('_')[0];
        if (token.StartsWith("DWD-", StringComparison.Ordinal)) token = token[4..];

        // Did the device come back with the primary or the secondary prefix?
        var deviceUsedSecondary = !string.IsNullOrEmpty(picked.SecondaryPrefix)
            && deviceReportedSsid.StartsWith(picked.SecondaryPrefix, StringComparison.Ordinal);

        if (deviceUsedSecondary && picked.ModelCodeWhenSecondaryUsed is not null)
        {
            return picked.ModelCodeWhenSecondaryUsed;
        }

        // The original _3() also checks m_B (the SSID prefix of the picked
        // product) to choose between two model codes for the same Tag suffix.
        // We model that with ModelCodeWhenAltPrefix.
        var matchingByTag = All.FirstOrDefault(p => p.Tag.Equals(token, StringComparison.OrdinalIgnoreCase));
        if (matchingByTag is null) return string.Empty;

        if (matchingByTag.ModelCodeWhenAltPrefix is not null
            && !picked.PrimaryPrefix.Equals(matchingByTag.PrimaryPrefix, StringComparison.Ordinal))
        {
            return matchingByTag.ModelCodeWhenAltPrefix;
        }
        return matchingByTag.ModelCode;
    }
}
