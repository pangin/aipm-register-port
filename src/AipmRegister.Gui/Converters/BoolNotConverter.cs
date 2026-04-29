using System.Globalization;
using Avalonia.Data.Converters;

namespace AipmRegister.Gui.Converters;

public sealed class BoolNotConverter : IValueConverter
{
    public static readonly BoolNotConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : true;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Convert(value, targetType, parameter, culture);
}
