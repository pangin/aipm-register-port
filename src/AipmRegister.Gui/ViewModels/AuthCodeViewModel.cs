using AipmRegister.Core.Notification;
using AipmRegister.Core.Orchestration;
using AipmRegister.Gui.Wizard;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AipmRegister.Gui.ViewModels;

public partial class AuthCodeViewModel : ObservableObject
{
    private readonly IRegistrationOrchestrator _orchestrator;
    private readonly IUserNotifier _notifier;
    private readonly IWizardNavigator _nav;
    private readonly WizardState _state;

    public AuthCodeViewModel(
        IRegistrationOrchestrator orchestrator,
        IUserNotifier notifier,
        IWizardNavigator nav,
        WizardState state)
    {
        _orchestrator = orchestrator;
        _notifier = notifier;
        _nav = nav;
        _state = state;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(VerifyCommand))]
    private string authCode = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    private string linkedAccount = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(VerifyCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextCommand))]
    private bool isBusy;

    [RelayCommand(CanExecute = nameof(CanVerify))]
    private async Task VerifyAsync()
    {
        IsBusy = true;
        try
        {
            var account = await _orchestrator.ExchangeAuthCodeAsync(AuthCode);
            if (account is null)
            {
                LinkedAccount = string.Empty;
                _notifier.Warn("Auth code is invalid or expired.");
                return;
            }
            _state.Account = account;
            _state.AuthCode = AuthCode;
            LinkedAccount = account.UserId;
        }
        catch (Exception ex)
        {
            _notifier.Error("Auth verification failed.", ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanVerify() => !IsBusy && AuthCode.Length == 8 && AuthCode.All(char.IsDigit);

    [RelayCommand(CanExecute = nameof(CanNext))]
    private void Next() => _nav.Go(WizardStep.ProductPicker);

    private bool CanNext() => !IsBusy && !string.IsNullOrEmpty(LinkedAccount);
}
