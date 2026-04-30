using AipmRegister.Gui.Localization;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;

namespace AipmRegister.Gui.Tests;

/// Boots a minimal Avalonia application under the headless platform so
/// XAML markup extensions (most importantly <see cref="LocExtension"/>)
/// resolve the same way they do at runtime — only the windowing
/// platform is swapped. Implements <see cref="ILocalizationProvider"/>
/// directly so headless tests don't need the production DI host;
/// LocExtension reaches the localization service via that interface
/// regardless of whether the running app is the production
/// <c>App</c> class or this <c>HeadlessApp</c>.
public sealed class HeadlessApp : Application, ILocalizationProvider
{
    public ILocalization? Localization { get; } = new AipmRegister.Gui.Localization.Localization();

    public override void Initialize() => Styles.Add(new FluentTheme());
}

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<HeadlessApp>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = true });
}

/// xUnit-compatible fixture wrapping an Avalonia headless session.
/// Tests that touch Avalonia objects join the <see cref="HeadlessCollection"/>
/// (via <c>[Collection(HeadlessCollection.Name)]</c>) and call
/// <c>Fixture.Run(() =&gt; ...)</c> so the action executes on the
/// dispatcher thread the headless platform expects.
///
/// A single <see cref="HeadlessUnitTestSession"/> is shared across every
/// test in the collection — the Avalonia headless platform initializes
/// global statics (the locator + dispatcher), so two parallel sessions
/// race each other and corrupt the locator's dictionary.
public sealed class HeadlessFixture : IDisposable
{
    public HeadlessUnitTestSession Session { get; }

    public HeadlessFixture() =>
        Session = HeadlessUnitTestSession.StartNew(typeof(TestAppBuilder));

    public Task Run(Action action) => Session.Dispatch(action, default);

    public void Dispose() => Session.Dispose();
}

[CollectionDefinition(Name)]
public sealed class HeadlessCollection : ICollectionFixture<HeadlessFixture>
{
    public const string Name = "Headless Avalonia";
}
