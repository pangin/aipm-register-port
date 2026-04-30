using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AipmRegister.Gui.Converters;

/// Maps the level field of a <c>LogEntry</c> ("info" / "warn" / "error")
/// to the foreground colour the terminal-style log panel uses.
/// Subdued grey for routine progress, amber for warnings, salmon-red
/// for errors — readable on a dark background.
public sealed class LogLevelToBrushConverter : IValueConverter
{
    public static readonly LogLevelToBrushConverter Instance = new();

    private static readonly IBrush Info  = new SolidColorBrush(Color.FromRgb(0xCF, 0xCF, 0xCF));
    private static readonly IBrush Warn  = new SolidColorBrush(Color.FromRgb(0xE5, 0xC0, 0x7B));
    private static readonly IBrush Error = new SolidColorBrush(Color.FromRgb(0xE0, 0x6C, 0x75));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && string.Equals(s, "warn",  StringComparison.OrdinalIgnoreCase) ? Warn
         : value is string e && string.Equals(e, "error", StringComparison.OrdinalIgnoreCase) ? Error
         : Info;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
