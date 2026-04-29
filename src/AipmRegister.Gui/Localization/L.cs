using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AipmRegister.Gui.Localization;

public enum AppLanguage { Ko, En }

/// Singleton localization source. Bind in XAML as
///     {Binding [Welcome.Body], Source={x:Static loc:L.Instance}}
/// and toggle by calling L.Instance.SetLanguage(AppLanguage.En).
public sealed class L : INotifyPropertyChanged
{
    public static L Instance { get; } = new();

    private AppLanguage _language = AppLanguage.Ko;

    public AppLanguage Language
    {
        get => _language;
        private set
        {
            if (_language == value) return;
            _language = value;
            OnPropertyChanged(nameof(Language));
            OnPropertyChanged("Item[]");                  // refresh every binding
            OnPropertyChanged(string.Empty);
        }
    }

    private IReadOnlyDictionary<string, string> CurrentDict
        => _language == AppLanguage.Ko ? Strings.Ko : Strings.En;

    /// XAML-friendly indexer. Returns the key itself when missing so the
    /// missing-resource shows up loudly in the UI.
    public string this[string key]
        => CurrentDict.TryGetValue(key, out var value) ? value : $"!{key}!";

    public void SetLanguage(AppLanguage lang) => Language = lang;
    public void Toggle() => Language = _language == AppLanguage.Ko ? AppLanguage.En : AppLanguage.Ko;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
