namespace AipmRegister.Gui.Localization;

/// Lets <see cref="LocExtension"/> reach an <see cref="ILocalization"/>
/// without taking a hard dependency on the concrete <c>App</c> class.
/// Production: <c>App</c> implements this interface and is registered as
/// <see cref="Avalonia.Application.Current"/>. Tests: a minimal
/// <c>TestApp</c> can implement it without spinning up the production
/// DI host.
public interface ILocalizationProvider
{
    ILocalization? Localization { get; }
}
