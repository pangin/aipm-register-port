namespace AipmRegister.Gui.Wizard;

public enum WizardStep { Welcome, WifiPicker, AuthCode, ProductPicker, DevicePicker, Registering }

public interface IWizardNavigator
{
    void Go(WizardStep step);
}
