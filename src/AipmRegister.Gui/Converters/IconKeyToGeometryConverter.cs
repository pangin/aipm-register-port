using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AipmRegister.Gui.Converters;

/// Looks up a StreamGeometry resource by key (e.g. "Icon.SmartPlug") in the
/// application's merged resource dictionaries (Assets/Icons.axaml).
public sealed class IconKeyToGeometryConverter : IValueConverter
{
    public static readonly IconKeyToGeometryConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string key) return null;
        if (Application.Current?.Resources.TryGetResource(key, null, out var resource) == true
            && resource is Geometry g)
        {
            return g;
        }
        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
