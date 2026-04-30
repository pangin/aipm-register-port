using AipmRegister.Core.Notification;

namespace AipmRegister.Gui.ViewModels;

/// Centralizes the IsBusy/try/catch/finally boilerplate that every wizard
/// ViewModel's RelayCommand handler used to repeat verbatim. Each step
/// VM now writes
///
///     [RelayCommand(CanExecute = nameof(CanX))]
///     private Task XAsync() =&gt; BusyAsync.RunAsync(
///         b =&gt; IsBusy = b, _notifier, "X failed.",
///         async () =&gt; { /* work */ });
///
/// instead of an open-coded six-line wrapper. The exception is
/// <c>RegisteringViewModel.RunAsync</c>, which has cancellation-token
/// state and multi-stage progress logic that doesn't fit a single
/// callback.
internal static class BusyAsync
{
    public static async Task RunAsync(
        Action<bool> setBusy,
        IUserNotifier notifier,
        string errorContext,
        Func<Task> work)
    {
        setBusy(true);
        try
        {
            await work();
        }
        catch (Exception ex)
        {
            notifier.Error(errorContext, ex);
        }
        finally
        {
            setBusy(false);
        }
    }
}
