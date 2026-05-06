using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace AipmRegister.Gui.Localization;

/// XAML markup extension: <code>{loc:Loc App.Title}</code> resolves to
/// the localized value for the given key, with live update on language
/// toggle.
///
/// Implementation note — manual reactive subscription, not a Binding.
/// Avalonia's <see cref="Avalonia.Data.Binding"/> with a string indexer
/// path (<c>"[Key]"</c>) does not respond to
/// <c>INotifyPropertyChanged("Item[]")</c> the way WPF's Binding does:
/// the BindingExpression subscribes to property-name notifications but
/// the indexer access doesn't refresh on the WPF-style "all-indexers"
/// signal. We saw that empirically — Title resolved correctly at parse
/// time but stayed put across <c>L.Toggle()</c>.
///
/// The fix: we hook the target's set-value path manually. ProvideValue
/// pushes the initial value, then subscribes to the localization
/// service's PropertyChanged event with a weak reference to the target
/// so a closed window doesn't leak through the singleton.
///
/// Bonus: this approach is AOT-friendly without reflection — the
/// indexer call is resolved at compile time, so PublishAot trimming
/// can statically prove the indexer is reachable.
public sealed class LocExtension : MarkupExtension
{
    public LocExtension() { }
    public LocExtension(string key) => Key = key;

    public string Key { get; set; } = string.Empty;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (serviceProvider is null) return $"!{Key}!";

        var loc = (Application.Current as ILocalizationProvider)?.Localization;
        if (loc is null) return $"!{Key}!";

        // Wire a live-update subscription for the next toggle when we
        // can identify the target (an AvaloniaObject + AvaloniaProperty
        // pair, which is exactly what every {loc:Loc Key} site provides).
        // The XAML loader assigns *this method's return value* to the
        // target at parse time, so we don't push the initial value
        // ourselves — we only handle subsequent language flips.
        var pvt = serviceProvider?.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        if (pvt?.TargetObject is AvaloniaObject ao && pvt.TargetProperty is AvaloniaProperty ap)
        {
            var weak = new WeakReference<AvaloniaObject>(ao);
            var key = Key;
            var prop = ap;
            PropertyChangedEventHandler? handler = null;
            handler = (_, _) =>
            {
                if (!weak.TryGetTarget(out var target))
                {
                    loc.PropertyChanged -= handler!;
                    return;
                }
                if (Dispatcher.UIThread.CheckAccess())
                {
                    target.SetValue(prop, loc[key]);
                }
                else
                {
                    Dispatcher.UIThread.Post(() => target.SetValue(prop, loc[key]));
                }
            };
            loc.PropertyChanged += handler;
        }

        // Initial value — XAML loader assigns it to the target.
        return loc[Key];
    }
}
