using AipmRegister.Core.Api;
using AipmRegister.Core.Devices;
using AipmRegister.Core.Models;
using AipmRegister.Core.Notification;
using AipmRegister.Core.Orchestration;
using AipmRegister.Core.Wifi;
using AipmRegister.Gui.Notification;
using AipmRegister.Gui.ViewModels;
using AipmRegister.Gui.Views;
using AipmRegister.Gui.Wifi;
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

        if (OperatingSystem.IsWindows())
        {
            builder.Services.AddSingleton<IWifiAdapter, AipmRegister.Wifi.Windows.WindowsWifiAdapter>();
        }
        else
        {
            builder.Services.AddSingleton<IWifiAdapter, NoopWifiAdapter>();
        }

        builder.Services.AddSingleton<IRegistrationOrchestrator, RegistrationOrchestrator>();
        builder.Services.AddSingleton<MainViewModel>();

        Host = builder.Build();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var vm = Host.Services.GetRequiredService<MainViewModel>();
            desktop.MainWindow = new MainWindow(vm);
            desktop.Exit += (_, _) =>
            {
                Host.Dispose();
                Host = null;
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
