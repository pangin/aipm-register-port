using System;
using Avalonia;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace AipmRegister.Gui.Localization;

/// XAML markup extension: <code>{loc:Loc App.Title}</code> resolves to the
/// localized value for the given key, with live update on language toggle.
///
/// Internally produces a <see cref="Binding"/> rooted at the
/// app-level <see cref="ILocalization"/> instance — so the
/// <c>"Item[]"</c> PropertyChanged plumbing in <c>Localization</c>
/// continues to refresh every binding when the language flips.
///
/// In designer mode (or any time the app's DI host hasn't built yet) the
/// extension returns a literal <c>"!{Key}!"</c> placeholder, matching the
/// runtime missing-key convention so the designer UI stays usable
/// without throwing.
public sealed class LocExtension : MarkupExtension
{
    public LocExtension() { }
    public LocExtension(string key) => Key = key;

    public string Key { get; set; } = string.Empty;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (Application.Current is App { Localization: { } loc })
        {
            return new Binding($"[{Key}]") { Source = loc };
        }
        return $"!{Key}!";
    }
}
