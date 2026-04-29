using AipmRegister.Core.Api;
using AipmRegister.Core.Devices;
using AipmRegister.Core.Models;
using AipmRegister.Core.Notification;
using AipmRegister.Core.Orchestration;
using AipmRegister.Core.Wifi;
using AipmRegister.Gui.Localization;
using AipmRegister.Gui.Notification;
using AipmRegister.Gui.ViewModels;
using AipmRegister.Gui.Views;
using AipmRegister.Gui.Wizard;
using AipmRegister.Hosting;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AipmRegister.Gui;

public partial class App : Application
{
    public IHost? Host { get; private set; }

    /// Resolved from the DI host once <c>OnFrameworkInitializationCompleted</c>
    /// finishes its <c>Host.Build()</c>. <see cref="LocExtension"/> reaches
    /// the service through this property; null until the build completes,
    /// in which case the markup extension renders a loud placeholder.
    public ILocalization? Localization { get; private set; }

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();

        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(o =>
        {
            o.SingleLine = true;
            o.IncludeScopes = false;
            o.TimestampFormat = "HH:mm:ss ";
        });

        builder.Services.AddSingleton(new BackendOptions());
        builder.Services.AddHttpClient<IRegisterApiClient, RegisterApiClient>();
        builder.Services.AddSingleton<IDeviceTcpSender, DeviceTcpSender>();
        builder.Services.AddSingleton<UiNotifier>();
        builder.Services.AddSingleton<IUserNotifier>(sp => sp.GetRequiredService<UiNotifier>());

        builder.Services.AddAipmWifiPlatform();

        builder.Services.AddSingleton<ILocalization, AipmRegister.Gui.Localization.Localization>();
        builder.Services.AddSingleton<IRegistrationOrchestrator, RegistrationOrchestrator>();

        // Wizard state + each step ViewModel + the main shell ViewModel.
        builder.Services.AddSingleton<WizardState>();
        builder.Services.AddSingleton<WizardNavigator>();
        builder.Services.AddSingleton<IWizardNavigator>(sp => sp.GetRequiredService<WizardNavigator>());
        builder.Services.AddSingleton<WelcomeViewModel>();
        builder.Services.AddSingleton<WifiPickerViewModel>();
        builder.Services.AddSingleton<AuthCodeViewModel>();
        builder.Services.AddSingleton<ProductPickerViewModel>();
        builder.Services.AddSingleton<DevicePickerViewModel>();
        builder.Services.AddSingleton<RegisteringViewModel>();
        builder.Services.AddSingleton<MainViewModel>();

        Host = builder.Build();
        Localization = Host.Services.GetRequiredService<ILocalization>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var vm = Host.Services.GetRequiredService<MainViewModel>();
            Host.Services.GetRequiredService<WizardNavigator>().SetHandler(vm.Go);
            desktop.MainWindow = new MainWindow { DataContext = vm };
            desktop.Exit += (_, _) =>
            {
                Host.Dispose();
                Host = null;
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
