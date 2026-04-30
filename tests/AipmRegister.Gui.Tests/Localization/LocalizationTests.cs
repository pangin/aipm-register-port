using System.ComponentModel;
using AipmRegister.Gui.Localization;

namespace AipmRegister.Gui.Tests.Localization;

public sealed class LocalizationTests
{
    [Fact]
    public void Indexer_ReturnsKoStringByDefault()
    {
        var loc = new AipmRegister.Gui.Localization.Localization();
        Assert.Equal(AppLanguage.Ko, loc.Language);
        Assert.Equal("AIPM Register", loc["App.Title"]);
    }

    [Fact]
    public void Indexer_ReturnsEnString_AfterSetLanguageEn()
    {
        var loc = new AipmRegister.Gui.Localization.Localization();
        loc.SetLanguage(AppLanguage.En);
        Assert.Equal("한국어", loc["Lang.Toggle"]);
        // The Ko dictionary's Lang.Toggle is "EN" (the toggle button shows
        // the *other* language); after switching to En it becomes "한국어".
        // Either way the string changes — assert Ko/En dictionaries
        // produce different values for at least one stable key.
        var ko = new AipmRegister.Gui.Localization.Localization();
        Assert.NotEqual(ko["App.SubTitle"], loc["App.SubTitle"]);
    }

    [Fact]
    public void Indexer_ReturnsBangKeyBang_WhenKeyMissing()
    {
        var loc = new AipmRegister.Gui.Localization.Localization();
        Assert.Equal("!Nope.Missing!", loc["Nope.Missing"]);
    }

    [Fact]
    public void Toggle_FlipsKoEnKo()
    {
        var loc = new AipmRegister.Gui.Localization.Localization();
        Assert.Equal(AppLanguage.Ko, loc.Language);
        loc.Toggle();
        Assert.Equal(AppLanguage.En, loc.Language);
        loc.Toggle();
        Assert.Equal(AppLanguage.Ko, loc.Language);
    }

    [Fact]
    public void SetLanguage_RaisesItemBracketsPropertyChanged()
    {
        var loc = new AipmRegister.Gui.Localization.Localization();
        var raised = new List<string?>();
        ((INotifyPropertyChanged)loc).PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        loc.SetLanguage(AppLanguage.En);

        // The XAML refresh contract: at minimum "Item[]" and "" must fire
        // so every indexer binding re-evaluates.
        Assert.Contains("Item[]", raised);
        Assert.Contains(string.Empty, raised);
    }
}
