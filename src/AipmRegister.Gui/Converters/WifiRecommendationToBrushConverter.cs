using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AipmRegister.Gui.Converters;

/// Mirrors frmMain.cs:1267 — IsRecommended==true paints rows GreenYellow,
/// otherwise Orange. The wizard's WifiPickerView binds row backgrounds via
/// this converter so the original visual cue carries over.
public sealed class WifiRecommendationToBrushConverter : IValueConverter
{
    public static readonly WifiRecommendationToBrushConverter Instance = new();

    private static readonly IBrush Green = new SolidColorBrush(Color.FromRgb(0xAD, 0xFF, 0x2F));
    private static readonly IBrush Orange = new SolidColorBrush(Color.FromRgb(0xFF, 0xA5, 0x00));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? Green : Orange;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
