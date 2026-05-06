using AipmRegister.Gui.Localization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace AipmRegister.Gui.Tests.Localization;

/// Headless-Avalonia regression tests for the <c>{loc:Loc Key}</c>
/// markup extension. The v1.4.0 → v1.5.1 → v1.7.0 saga taught us that
/// a clean `dotnet build` is not enough: the original Binding-from-
/// string constructor compiled fine but did not parse the indexer
/// path correctly; the property-initializer Binding form parsed
/// correctly at startup but ignored language-toggle PropertyChanged
/// notifications. v1.7.1 swaps to a manual reactive subscription —
/// these tests pin both the parse-time value and the live update
/// contract directly.
[Collection(HeadlessCollection.Name)]
public sealed class LocExtensionTests
{
    private readonly HeadlessFixture _headless;
    public LocExtensionTests(HeadlessFixture headless) => _headless = headless;

    [Fact]
    public Task ProvideValue_ReturnsCurrentLocalizedString()
        => _headless.Run(() =>
        {
            var loc = ((ILocalizationProvider)Application.Current!).Localization!;
            loc.SetLanguage(AppLanguage.Ko);

            var tb = new TextBlock();
            var sp = new StubServiceProvider(tb, TextBlock.TextProperty);

            // Mirrors what the XAML loader does: it takes ProvideValue's
            // return and assigns it to the target.
            var initial = new LocExtension("App.Title").ProvideValue(sp);
            tb.SetValue(TextBlock.TextProperty, initial);

            Assert.Equal("AIPM Register", tb.Text);
        });

    [Fact]
    public Task ProvideValue_LiveUpdatesOnLanguageToggle()
        => _headless.Run(() =>
        {
            var loc = ((ILocalizationProvider)Application.Current!).Localization!;
            loc.SetLanguage(AppLanguage.Ko);

            var tb = new TextBlock();
            var sp = new StubServiceProvider(tb, TextBlock.TextProperty);

            var initial = new LocExtension("Lang.Toggle").ProvideValue(sp);
            tb.SetValue(TextBlock.TextProperty, initial);
            Assert.Equal("EN", tb.Text);

            // Subsequent toggles must propagate to tb.Text via the
            // manual subscription LocExtension wires up — without going
            // through Avalonia's binding pipeline (which silently
            // ignores `Item[]` PropertyChanged on string-indexer paths).
            loc.Toggle();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("한국어", tb.Text);

            loc.Toggle();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal("EN", tb.Text);
        });

    [Fact]
    public void ProvideValue_ReturnsBangPlaceholder_WhenLocalizationProviderIsAbsent()
    {
        // No XAML service provider means designer / pre-DI-bootstrap path.
        // The extension returns a literal placeholder instead of touching
        // Avalonia's global Application.Current.
        var ext = new LocExtension("App.Title");
        var result = ext.ProvideValue(serviceProvider: null!);
        Assert.Equal("!App.Title!", result);
    }

    /// Minimal IServiceProvider that hands back an IProvideValueTarget
    /// pointing at the requested AvaloniaObject + AvaloniaProperty —
    /// just what <see cref="LocExtension.ProvideValue"/> needs to wire
    /// the manual subscription.
    private sealed class StubServiceProvider : IServiceProvider, IProvideValueTarget
    {
        public StubServiceProvider(object target, object property)
        {
            TargetObject = target;
            TargetProperty = property;
        }
        public object TargetObject { get; }
        public object TargetProperty { get; }
        public object? GetService(Type serviceType)
            => serviceType == typeof(IProvideValueTarget) ? this : null;
    }
}
