using AipmRegister.Gui.Wizard;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AipmRegister.Gui.ViewModels;

public partial class WelcomeViewModel : ObservableObject
{
    private readonly IWizardNavigator _nav;
    public WelcomeViewModel(IWizardNavigator nav) => _nav = nav;

    [RelayCommand]
    private void Start() => _nav.Go(WizardStep.WifiPicker);
}
