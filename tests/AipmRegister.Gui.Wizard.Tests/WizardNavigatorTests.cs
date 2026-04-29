using AipmRegister.Gui.Wizard;

namespace AipmRegister.Gui.Wizard.Tests;

public class WizardNavigatorTests
{
    [Fact]
    public void Go_BeforeSetHandler_Throws()
    {
        var nav = new WizardNavigator();
        var ex = Assert.Throws<InvalidOperationException>(() => nav.Go(WizardStep.WifiPicker));
        Assert.Contains(nameof(WizardNavigator.SetHandler), ex.Message);
    }

    [Fact]
    public void Go_AfterSetHandler_InvokesHandlerWithStep()
    {
        var nav = new WizardNavigator();
        WizardStep? received = null;
        nav.SetHandler(s => received = s);

        nav.Go(WizardStep.AuthCode);

        Assert.Equal(WizardStep.AuthCode, received);
    }

    [Fact]
    public void SetHandler_Null_Throws()
    {
        var nav = new WizardNavigator();
        Assert.Throws<ArgumentNullException>(() => nav.SetHandler(null!));
    }

    [Fact]
    public void SetHandler_Twice_LatestWins()
    {
        var nav = new WizardNavigator();
        var firstCalls = 0;
        var secondCalls = 0;
        nav.SetHandler(_ => firstCalls++);
        nav.SetHandler(_ => secondCalls++);

        nav.Go(WizardStep.Welcome);

        Assert.Equal(0, firstCalls);
        Assert.Equal(1, secondCalls);
    }
}
