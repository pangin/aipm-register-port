using AipmRegister.Gui.Localization;
using AipmRegister.Gui.Wizard;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AipmRegister.Gui.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ILocalization _l;

    public MainViewModel(
        WelcomeViewModel welcome,
        WifiPickerViewModel wifi,
        AuthCodeViewModel auth,
        ProductPickerViewModel product,
        DevicePickerViewModel device,
        RegisteringViewModel registering,
        ILocalization l)
    {
        Welcome = welcome;
        Wifi = wifi;
        Auth = auth;
        Product = product;
        Device = device;
        Registering = registering;
        _l = l;
        Current = welcome;
    }

    public WelcomeViewModel       Welcome     { get; }
    public WifiPickerViewModel    Wifi        { get; }
    public AuthCodeViewModel      Auth        { get; }
    public ProductPickerViewModel Product     { get; }
    public DevicePickerViewModel  Device      { get; }
    public RegisteringViewModel   Registering { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StepIndex))]
    [NotifyPropertyChangedFor(nameof(IsWizardStep))]
    private object current;

    [ObservableProperty] private WizardStep currentStep = WizardStep.Welcome;

    /// 1-based index used in headers ("(N/5)"). 0 means we're on the welcome screen.
    public int StepIndex => CurrentStep switch
    {
        WizardStep.WifiPicker    => 1,
        WizardStep.AuthCode      => 2,
        WizardStep.ProductPicker => 3,
        WizardStep.DevicePicker  => 4,
        WizardStep.Registering   => 5,
        _                        => 0,
    };

    public bool IsWizardStep => CurrentStep != WizardStep.Welcome;

    public void Go(WizardStep step)
    {
        CurrentStep = step;
        Current = step switch
        {
            WizardStep.Welcome       => Welcome,
            WizardStep.WifiPicker    => Wifi,
            WizardStep.AuthCode      => Auth,
            WizardStep.ProductPicker => Product,
            WizardStep.DevicePicker  => Device,
            WizardStep.Registering   => Registering,
            _                        => Welcome,
        };

        // Step-on-enter side effects.
        if (step == WizardStep.WifiPicker)    _ = Wifi.PrimeAsync();
        if (step == WizardStep.DevicePicker)  _ = Device.PrimeAsync();
        if (step == WizardStep.Registering)   _ = Registering.RunAsync();
    }

    [RelayCommand]
    private void ToggleLanguage() => _l.Toggle();
}
