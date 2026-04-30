using System;
using System.Diagnostics.CodeAnalysis;
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

    // Avalonia's Binding(string) ctor flags IL2026/IL3050 because string-
    // path bindings normally use reflection. Our path is the trivial
    // indexer "[Key]" rooted at ILocalization, which has exactly one
    // indexer (string-keyed). Trimming preserves it because Localization
    // is reachable through DI; AOT inlines the indexer call. Suppress
    // both with that justification.
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members which require dynamic access cannot be statically analyzed.",
        Justification = "Bind path is a single string-indexer on ILocalization which is preserved via DI registration.")]
    [UnconditionalSuppressMessage("AOT", "IL3050:Avoid calling members with RequiresDynamicCodeAttribute.",
        Justification = "Bind path is a single string-indexer on ILocalization which is preserved via DI registration.")]
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var loc = (Application.Current as ILocalizationProvider)?.Localization;
        if (loc is null) return $"!{Key}!";

        // Wrap the indexer access in a ReflectionBindingExtension /
        // CompiledBinding-equivalent: we set up the Binding via property
        // initializers (not the string-path constructor) so the
        // path-parser visits the indexer node directly, which is what the
        // original `{Binding [Key], Source=L.Instance}` did pre-refactor.
        var binding = new Binding
        {
            Source = loc,
            Path   = $"[{Key}]",
            Mode   = BindingMode.OneWay,
        };
        return binding;
    }
}
