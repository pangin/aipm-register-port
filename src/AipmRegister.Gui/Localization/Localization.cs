using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AipmRegister.Gui.Localization;

/// Default <see cref="ILocalization"/> implementation. Instances are
/// DI-registered as singletons by <c>App.OnFrameworkInitializationCompleted</c>;
/// no <c>Instance</c> static field — the static singleton anti-pattern that
/// lived on the previous <c>L</c> class is gone, and each consumer (VM,
/// converter, markup extension) reaches the singleton through the DI
/// container or the framework-level <c>Application.Current</c> handle.
public sealed class Localization : ILocalization
{
    private AppLanguage _language = AppLanguage.Ko;

    public AppLanguage Language
    {
        get => _language;
        private set
        {
            if (_language == value) return;
            _language = value;
            OnPropertyChanged(nameof(Language));
            OnPropertyChanged("Item[]");                  // refresh every indexer binding
            OnPropertyChanged(string.Empty);
        }
    }

    private IReadOnlyDictionary<string, string> CurrentDict
        => _language == AppLanguage.Ko ? Strings.Ko : Strings.En;

    public string this[string key]
        => CurrentDict.TryGetValue(key, out var value) ? value : $"!{key}!";

    public void SetLanguage(AppLanguage lang) => Language = lang;

    public void Toggle()
        => Language = _language == AppLanguage.Ko ? AppLanguage.En : AppLanguage.Ko;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
