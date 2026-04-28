using AipmRegister.Cli;
using AipmRegister.Cli.Wifi;
using AipmRegister.Core.Api;
using AipmRegister.Core.Devices;
using AipmRegister.Core.Models;
using AipmRegister.Core.Notification;
using AipmRegister.Core.Orchestration;
using AipmRegister.Core.Wifi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;

var authOption     = new Option<string>("--auth-code")               { Required = true,  Description = "8-digit auth code from the mobile app." };
var hotspotOption  = new Option<string>("--device-hotspot-ssid")     { Required = true,  Description = "Device hotspot SSID (e.g. DAWON_IRBD_xxxx, DWD-yyyy)." };
var homeSsidOpt    = new Option<string>("--home-ssid")               { Required = true,  Description = "Home Wi-Fi SSID the device should join." };
var homePassOpt    = new Option<string>("--home-password")           { Required = true,  Description = "Home Wi-Fi password (sent to device over local TCP)." };
var hotspotPassOpt = new Option<string>("--device-hotspot-password") { Required = false, Description = "Hotspot password (open networks usually leave blank).", DefaultValueFactory = _ => string.Empty };
var hostOption     = new Option<string>("--device-host")             { Required = false, Description = "Device IP on its hotspot.", DefaultValueFactory = _ => "192.168.4.1" };
var portOption     = new Option<int>("--device-port")                { Required = false, Description = "Device TCP port.", DefaultValueFactory = _ => 5000 };
var attemptsOpt    = new Option<int>("--max-attempts")               { Required = false, Description = "Max control/check polls.", DefaultValueFactory = _ => 30 };
var pollOption     = new Option<int>("--poll-seconds")               { Required = false, Description = "Seconds between polls.", DefaultValueFactory = _ => 2 };

var root = new RootCommand("Register a DAWON IoT device. Replaces the original Windows-only AIPM_Register.exe with a cross-platform CLI.")
{
    authOption, hotspotOption, homeSsidOpt, homePassOpt, hotspotPassOpt,
    hostOption, portOption, attemptsOpt, pollOption,
};

root.SetAction(async (parseResult, ct) =>
{
    var request = new RegistrationRequest(
        AuthCode8Digits:        parseResult.GetValue(authOption)!,
        HomeSsid:               parseResult.GetValue(homeSsidOpt)!,
        HomePassword:           parseResult.GetValue(homePassOpt)!,
        DeviceHotspotSsid:      parseResult.GetValue(hotspotOption)!,
        DeviceHotspotPassword:  parseResult.GetValue(hotspotPassOpt) ?? string.Empty,
        DeviceTcpHost:          parseResult.GetValue(hostOption)!,
        DeviceTcpPort:          parseResult.GetValue(portOption),
        MaxControlCheckAttempts:parseResult.GetValue(attemptsOpt),
        PollInterval:           TimeSpan.FromSeconds(parseResult.GetValue(pollOption)));

    var host = Host.CreateApplicationBuilder();
    host.Logging.ClearProviders();
    host.Logging.AddSimpleConsole(o =>
    {
        o.SingleLine = true;
        o.IncludeScopes = false;
        o.TimestampFormat = "HH:mm:ss ";
    });

    host.Services.AddSingleton(new BackendOptions());
    host.Services.AddHttpClient<IRegisterApiClient, RegisterApiClient>();
    host.Services.AddSingleton<IDeviceTcpSender, DeviceTcpSender>();
    host.Services.AddSingleton<IUserNotifier, ConsoleNotifier>();

    if (OperatingSystem.IsWindows())
    {
        host.Services.AddSingleton<IWifiAdapter, AipmRegister.Cli.Wifi.Windows.WindowsWifiAdapter>();
    }
    else
    {
        host.Services.AddSingleton<IWifiAdapter, NoopWifiAdapter>();
    }
    host.Services.AddSingleton<IRegistrationOrchestrator, RegistrationOrchestrator>();

    using var app = host.Build();
    var orchestrator = app.Services.GetRequiredService<IRegistrationOrchestrator>();

    var result = await orchestrator.RunAsync(request, ct);

    Console.WriteLine();
    Console.WriteLine("=== result ===");
    Console.WriteLine($"status     : {result.Status}");
    Console.WriteLine($"user_id    : {result.UserId ?? "-"}");
    Console.WriteLine($"device_id  : {result.DeviceId ?? "-"}");
    Console.WriteLine($"message    : {result.Message ?? "-"}");

    return result.Status == RegistrationStatus.Succeeded ? 0 : 1;
});

return await root.Parse(args).InvokeAsync();
