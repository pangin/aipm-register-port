using System.Collections.Concurrent;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace AipmRegister.Gui.Converters;

/// Loads the embedded product photo named "{key}.png" from
/// avares://AipmRegister.Gui/Assets/Products/. Bitmaps are cached so the
/// ProductPickerView can switch templates / language without reopening
/// the same stream every time.
public sealed class PhotoKeyToBitmapConverter : IValueConverter
{
    public static readonly PhotoKeyToBitmapConverter Instance = new();

    private static readonly ConcurrentDictionary<string, Bitmap> Cache = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string key || string.IsNullOrEmpty(key)) return null;

        return Cache.GetOrAdd(key, static k =>
        {
            var uri = new Uri($"avares://AipmRegister.Gui/Assets/Products/{k}.png");
            using var stream = AssetLoader.Open(uri);
            return new Bitmap(stream);
        });
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
