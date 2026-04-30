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
    /// Filename stem of the product photo embedded as
    /// avares://AipmRegister.Gui/Assets/Products/{PhotoKey}.png. Recovered
    /// from the original frmMain.cs ImageList SetKeyName mapping
    /// (lines 371–383). Several SKUs deliberately share one photo — e.g.
    /// the 100A/400A/800A panelboards all reused index 8 = "P230".
    string PhotoKey,
    /// Primary hotspot SSID prefix (always "DWD-" + Tag).
    string PrimaryPrefix,
    /// Secondary prefix (some SKUs share two hotspot families).
    /// Empty when the SKU has only the primary.
    string SecondaryPrefix,
    /// Model code used when the selected hotspot matches the primary prefix.
    string ModelCode,
    /// Optional override used when the selected hotspot matches the secondary
    /// prefix. These are the odd legacy _3() mappings where the scanned SSID
    /// token differs from the picked product tag.
    string? ModelCodeWhenSecondaryUsed = null);

public static class ProductCatalog
{
    /// All 15 SKUs in the order the original ListView shows them.
    public static IReadOnlyList<ProductDefinition> All { get; } = new[]
    {
        new ProductDefinition("S120",   "Product.SmartPlug16A.S120.Name",      "Icon.SmartPlug",   "B540",   "DWD-S120",   "DWD-LS120", "B530_W",
            ModelCodeWhenSecondaryUsed: "B540_W"),
        new ProductDefinition("ES120",  "Product.SmartPlug16A.ES120.Name",     "Icon.SmartPlug",   "B550E",  "DWD-ES120",  "DWD-SS120", "B550E_W",
            ModelCodeWhenSecondaryUsed: "B550_W"),
        new ProductDefinition("LS130",  "Product.SmartPlug16A.LS130.Name",     "Icon.SmartPlug",   "B350",   "DWD-LS130",  "",          "B350_W"),
        new ProductDefinition("S220",   "Product.SmartMultitap16A.Name",       "Icon.MultiTap",    "M130",   "DWD-S220",   "",          "M130_W"),
        new ProductDefinition("LS810",  "Product.ZigbeeHub.Name",              "Icon.ZigbeeHub",   "G200L",  "DWD-LS810",  "",          "G200L_ZB"),
        new ProductDefinition("S510",   "Product.IrRemote.S510.Name",          "Icon.IrRemote",    "R110",   "DWD-S510",   "",          "R110_W"),
        new ProductDefinition("S501",   "Product.IrRemote.S501.Name",          "Icon.IrRemote",    "R200",   "DWD-S501",   "DWD-S510",  "R200_W",
            ModelCodeWhenSecondaryUsed: "R200_W"),
        new ProductDefinition("S310",   "Product.PanelboardSingle50A.Name",    "Icon.Panelboard",  "P110",   "DWD-S310",   "DWD-S311",  "P110_W",
            ModelCodeWhenSecondaryUsed: "P110_WA"),
        new ProductDefinition("S330",   "Product.PanelboardThree100A.Name",    "Icon.Panelboard",  "P230",   "DWD-S330",   "",          "P230_W"),
        // S350 / S370 deliberately reuse P230 — the original ListView mapped
        // 100A / 400A / 800A all to ImageList index 8.
        new ProductDefinition("S350",   "Product.PanelboardThree400A.Name",    "Icon.Panelboard",  "P230",   "DWD-S350",   "",          "P250_W"),
        new ProductDefinition("S370",   "Product.PanelboardThree800A.Name",    "Icon.Panelboard",  "P230",   "DWD-S370",   "",          "P270_W"),
        new ProductDefinition("ES120S", "Product.SolarSmartPlug16A.Name",      "Icon.SolarPlug",   "B550ES", "DWD-ES120S", "DWD-ES120", "B550E_SW",
            ModelCodeWhenSecondaryUsed: "B550E_SW"),
        new ProductDefinition("S600",   "Product.SolarSmartPlug10A.Name",      "Icon.SolarPlug",   "B400S",  "DWD-S600",   "",          "B400_SW"),
        new ProductDefinition("S110",   "Product.SmartPlug10A.Name",           "Icon.SmartPlug",   "B400",   "DWD-S110",   "DWD-S600",  "B400_WI",
            ModelCodeWhenSecondaryUsed: "B400_W"),
        new ProductDefinition("S121",   "Product.SmartPlugJP.Name",            "Icon.SmartPlug",   "B343",   "DWD-S121",   "",          "B343_W"),
    };

    /// Returns true iff `ssid` could be a hotspot of `product` (i.e. starts
    /// with either of its prefixes).
    public static bool IsHotspotOf(this ProductDefinition product, string ssid)
        => ssid.StartsWith(product.PrimaryPrefix, StringComparison.Ordinal)
            || (!string.IsNullOrEmpty(product.SecondaryPrefix)
                && ssid.StartsWith(product.SecondaryPrefix, StringComparison.Ordinal));

    /// Resolve the model code recovered from a selected device hotspot SSID such as
    /// "DWD-S120_3b12b9". Mirrors frmMain.cs _3() (line 2106) but uses the
    /// catalog instead of a hand-rolled switch.
    public static string ResolveModelCode(string deviceHotspotSsid, ProductDefinition picked)
    {
        var prefix = deviceHotspotSsid.Split('_')[0];

        if (!string.IsNullOrEmpty(picked.SecondaryPrefix)
            && prefix.Equals(picked.SecondaryPrefix, StringComparison.Ordinal)
            && picked.ModelCodeWhenSecondaryUsed is not null)
        {
            return picked.ModelCodeWhenSecondaryUsed;
        }

        var matchingByPrimaryPrefix = All.FirstOrDefault(p =>
            p.PrimaryPrefix.Equals(prefix, StringComparison.Ordinal));
        return matchingByPrimaryPrefix?.ModelCode ?? picked.ModelCode;
    }
}
