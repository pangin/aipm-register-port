using System.Globalization;
using Avalonia.Data.Converters;

namespace AipmRegister.Gui.Converters;

/// Returns true when the bound string is null or empty. Used by
/// ProductPickerView to swap a fallback icon in for SKUs that have no
/// embedded product photo (the HotspotSsidParser unknown-SKU path).
public sealed class StringIsNullOrEmptyConverter : IValueConverter
{
    public static readonly StringIsNullOrEmptyConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is not string s || string.IsNullOrEmpty(s);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
