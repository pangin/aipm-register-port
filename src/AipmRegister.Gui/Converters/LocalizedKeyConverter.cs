using System.Globalization;
using AipmRegister.Gui.Localization;
using Avalonia.Data.Converters;

namespace AipmRegister.Gui.Converters;

/// Converts a localization key (e.g. "Product.SmartPlug16A.S120.Name") into
/// the localized string from L.Instance — useful when binding inside a
/// DataTemplate where we cannot easily reach L.Instance via a regular Source.
public sealed class LocalizedKeyConverter : IValueConverter
{
    public static readonly LocalizedKeyConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string key ? L.Instance[key] : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
