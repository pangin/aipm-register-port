namespace AipmRegister.Core.Models;

/// Shared helpers for picking apart a DAWON device hotspot SSID. Originally
/// inlined as private statics in <c>RegistrationOrchestrator</c> and
/// duplicated as <c>ExtractMac</c> in the GUI's DevicePickerViewModel —
/// extracted here so both call sites use one canonical implementation.
///
/// The recovered SSID conventions (frmMain.cs lines 1862-1864 + analysis
/// of the deployed catalog):
///   - "DAWON_IRBD_AABBCC"  → product prefix "DAWON_IRBD", MAC tail "AABBCC"
///   - "DWD-S120_3b12b9"    → product prefix "DWD-S120",   MAC tail "3b12b9"
///
/// MAC extraction tolerates both `_` and `-` as the boundary; product
/// matching walks <see cref="ProductCatalog.All"/> first and falls back
/// to a synthetic <see cref="ProductDefinition"/> built from the leading
/// SSID token so unknown SKUs still flow through the CLI without
/// hard-failing.
public static class HotspotSsidParser
{
    /// Walks the catalog and picks whatever product owns the given hotspot
    /// SSID. Falls back to a synthetic <see cref="ProductDefinition"/>
    /// built from the SSID prefix so the CLI can still target unknown
    /// SKUs.
    public static ProductDefinition ResolveProduct(string ssid)
    {
        foreach (var p in ProductCatalog.All)
        {
            if (p.IsHotspotOf(ssid)) return p;
        }
        var token = ssid.Split('_')[0];
        return new ProductDefinition(
            Tag:             token.StartsWith("DWD-", StringComparison.Ordinal) ? token[4..] : token,
            DisplayKey:      "Product.Unknown.Name",
            IconKey:         "Icon.SmartPlug",
            PrimaryPrefix:   token,
            SecondaryPrefix: string.Empty,
            ModelCode:       "UNKNOWN");
    }

    /// "DAWON_IRBD_AABBCC" → "AABBCC", "DWD-S120_3b12b9" → "3b12b9".
    /// The trailing token after the last `_` or `-` is the device MAC
    /// suffix.
    public static string ExtractMac(string hotspotSsid)
    {
        var idx = hotspotSsid.LastIndexOfAny(new[] { '_', '-' });
        return idx >= 0 && idx < hotspotSsid.Length - 1
            ? hotspotSsid[(idx + 1)..]
            : hotspotSsid;
    }
}
