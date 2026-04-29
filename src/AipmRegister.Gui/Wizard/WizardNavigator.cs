using System;

namespace AipmRegister.Gui.Wizard;

/// Adapter that decouples wizard step requests (raised by child ViewModels via
/// IWizardNavigator) from the concrete handler (the MainViewModel). Wired in
/// App.OnFrameworkInitializationCompleted after MainViewModel has been
/// resolved, so MainViewModel itself does not implement IWizardNavigator —
/// doing so would re-introduce a singleton-on-singleton DI cycle that
/// deadlocks ServiceProvider during the first MainViewModel resolution.
public sealed class WizardNavigator : IWizardNavigator
{
    private Action<WizardStep>? _handler;

    public void SetHandler(Action<WizardStep> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _handler = handler;
    }

    public void Go(WizardStep step)
    {
        if (_handler is null)
        {
            throw new InvalidOperationException(
                $"{nameof(WizardNavigator)}.{nameof(Go)} called before " +
                $"{nameof(SetHandler)}. Wire the handler during App startup.");
        }
        _handler(step);
    }
}
