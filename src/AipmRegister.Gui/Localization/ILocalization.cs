using System.ComponentModel;

namespace AipmRegister.Gui.Localization;

public enum AppLanguage { Ko, En }

/// Localization service consumed by ViewModels (via DI) and by XAML
/// (indirectly through <see cref="LocExtension"/>). Exposes an indexer
/// keyed by the stable string IDs in <c>Strings.Ko</c> / <c>Strings.En</c>
/// and raises <see cref="INotifyPropertyChanged.PropertyChanged"/> with
/// the magic <c>"Item[]"</c> name so every XAML indexer binding refreshes
/// in lock-step with <see cref="Toggle"/>.
public interface ILocalization : INotifyPropertyChanged
{
    /// Returns the localized value for <paramref name="key"/>; falls back
    /// to <c>"!{key}!"</c> when missing so the gap is loud in the UI.
    string this[string key] { get; }

    AppLanguage Language { get; }

    void SetLanguage(AppLanguage language);

    /// Flips Ko ↔ En. Idempotent across two consecutive calls.
    void Toggle();
}
