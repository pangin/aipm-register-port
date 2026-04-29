using System.Globalization;
using AipmRegister.Gui.Localization;
using Avalonia;
using Avalonia.Data.Converters;

namespace AipmRegister.Gui.Converters;

/// Converts a localization key (e.g. "Product.SmartPlug16A.S120.Name") into
/// the localized string from the app-level <see cref="ILocalization"/> —
/// used when binding inside a DataTemplate where the per-item key is the
/// bound value and a markup-extension would not see it.
///
/// The static <c>Instance</c> field is preserved because the call site is
/// <c>{x:Static conv:LocalizedKeyConverter.Instance}</c> in XAML; the
/// conversion itself reaches the localization service through
/// <c>Application.Current</c>, so no static singleton lives on the
/// localization side.
public sealed class LocalizedKeyConverter : IValueConverter
{
    public static readonly LocalizedKeyConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string key) return string.Empty;
        if (Application.Current is App { Localization: { } loc }) return loc[key];
        return $"!{key}!";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
