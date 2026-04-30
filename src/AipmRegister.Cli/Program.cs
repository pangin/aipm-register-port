using AipmRegister.Cli;
using AipmRegister.Core.Api;
using AipmRegister.Core.Devices;
using AipmRegister.Core.Models;
using AipmRegister.Core.Network;
using AipmRegister.Core.Notification;
using AipmRegister.Core.Orchestration;
using AipmRegister.Core.Wifi;
using AipmRegister.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;

var interactiveOpt = new Option<bool>("--interactive")
{
    Required = false,
    Description = "Run the GUI-like step-by-step flow. This is also the default when required values are missing.",
};
var verboseOpt = new Option<bool>("--verbose")
{
    Required = false,
    Description = "Print debug logs, including TCP replies and platform Wi-Fi diagnostics.",
};
var authOption = new Option<string?>("--auth-code")
{
    Required = false,
    Description = "8-digit auth code from the mobile app.",
};
var hotspotOption = new Option<string?>("--device-hotspot-ssid")
{
    Required = false,
    Description = "Device hotspot SSID (e.g. DWD-S120_AABBCC).",
};
var homeSsidOpt = new Option<string?>("--home-ssid")
{
    Required = false,
    Description = "Home Wi-Fi SSID the device should join.",
};
var homePassOpt = new Option<string?>("--home-password")
{
    Required = false,
    Description = "Home Wi-Fi password (sent to device over local TCP).",
};
var homeSecurityOpt = new Option<WifiSecurity>("--home-security")
{
    Required = false,
    Description = "Home Wi-Fi security used when reconnecting after the device handoff.",
    DefaultValueFactory = _ => WifiSecurity.Wpa2Personal,
};
var hotspotPassOpt = new Option<string>("--device-hotspot-password")
{
    Required = false,
    Description = "Hotspot password (open networks usually leave blank).",
    DefaultValueFactory = _ => string.Empty,
};
var hostOption = new Option<string>("--device-host")
{
    Required = false,
    Description = "Device IP on its hotspot. The Wi-Fi gateway can override this on supported platforms.",
    DefaultValueFactory = _ => "192.168.4.1",
};
var portOption = new Option<int>("--device-port")
{
    Required = false,
    Description = "Device TCP port.",
    DefaultValueFactory = _ => 5000,
};
var attemptsOpt = new Option<int>("--max-attempts")
{
    Required = false,
    Description = "Max control/check polls.",
    DefaultValueFactory = _ => 30,
};
var pollOption = new Option<int>("--poll-seconds")
{
    Required = false,
    Description = "Seconds between polls.",
    DefaultValueFactory = _ => 2,
};
var ifaceOption = new Option<string?>("--wifi-interface")
{
    Required = false,
    Description = "Wireless interface id to use (e.g. wlan0, en0, GUID on Windows). AIPM_WIFI_IFACE is honoured as a fallback.",
};

var root = new RootCommand("Register a DAWON IoT device. With no arguments, starts the GUI-like interactive verifier.")
{
    interactiveOpt,
    verboseOpt,
    authOption,
    hotspotOption,
    homeSsidOpt,
    homePassOpt,
    homeSecurityOpt,
    hotspotPassOpt,
    hostOption,
    portOption,
    attemptsOpt,
    pollOption,
    ifaceOption,
};

root.SetAction(async (parseResult, ct) =>
{
    var options = new CliRegistrationOptions(
        AuthCode: parseResult.GetValue(authOption),
        DeviceHotspotSsid: parseResult.GetValue(hotspotOption),
        HomeSsid: parseResult.GetValue(homeSsidOpt),
        HomePassword: parseResult.GetValue(homePassOpt),
        DeviceHotspotPassword: parseResult.GetValue(hotspotPassOpt) ?? string.Empty,
        DeviceHost: parseResult.GetValue(hostOption) ?? "192.168.4.1",
        DevicePort: parseResult.GetValue(portOption),
        MaxAttempts: parseResult.GetValue(attemptsOpt),
        PollInterval: TimeSpan.FromSeconds(parseResult.GetValue(pollOption)),
        WifiInterface: parseResult.GetValue(ifaceOption)
                       ?? Environment.GetEnvironmentVariable("AIPM_WIFI_IFACE"),
        HomeSecurity: parseResult.GetValue(homeSecurityOpt),
        Verbose: parseResult.GetValue(verboseOpt));

    using var app = BuildApp(options.Verbose);

    var shouldRunInteractive = parseResult.GetValue(interactiveOpt)
                               || args.Length == 0
                               || !options.HasHeadlessRequiredFields;
    if (shouldRunInteractive)
    {
        return await InteractiveRegistrationRunner.RunAsync(app.Services, options, ct);
    }

    return await RunHeadlessAsync(app.Services, options, ct);
});

return await root.Parse(args).InvokeAsync();

static IHost BuildApp(bool verbose)
{
    var host = Host.CreateApplicationBuilder();
    host.Logging.ClearProviders();
    host.Logging.SetMinimumLevel(verbose ? LogLevel.Debug : LogLevel.Information);
    host.Logging.AddSimpleConsole(o =>
    {
        o.SingleLine = true;
        o.IncludeScopes = false;
        o.TimestampFormat = "HH:mm:ss ";
    });

    host.Services.AddSingleton(new BackendOptions());
    host.Services.AddHttpClient<IRegisterApiClient, RegisterApiClient>();
    host.Services.AddSingleton<IDeviceTcpSender, DeviceTcpSender>();
    host.Services.AddSingleton<IInternetReachabilityProbe, TcpInternetReachabilityProbe>();
    host.Services.AddSingleton<IUserNotifier, ConsoleNotifier>();
    host.Services.AddAipmWifiPlatform();
    host.Services.AddSingleton<IRegistrationOrchestrator, RegistrationOrchestrator>();

    return host.Build();
}

static async Task<int> RunHeadlessAsync(
    IServiceProvider services,
    CliRegistrationOptions options,
    CancellationToken ct)
{
    var request = new RegistrationRequest(
        AuthCode8Digits: options.AuthCode!,
        HomeSsid: options.HomeSsid!,
        HomePassword: options.HomePassword!,
        DeviceHotspotSsid: options.DeviceHotspotSsid!,
        DeviceHotspotPassword: options.DeviceHotspotPassword,
        DeviceTcpHost: options.DeviceHost,
        DeviceTcpPort: options.DevicePort,
        MaxControlCheckAttempts: options.MaxAttempts,
        PollInterval: options.PollInterval,
        HomeSecurity: options.HomeSecurity);

    var enumerator = services.GetRequiredService<IWifiInterfaceEnumerator>();
    var factory = services.GetRequiredService<IWifiAdapterFactory>();
    var picked = await InteractiveRegistrationRunner.PickInterfaceAsync(enumerator, options.WifiInterface, ct);
    if (picked is null) return 2;

    var wifi = factory.Create(picked);
    var orchestrator = services.GetRequiredService<IRegistrationOrchestrator>();
    var result = await orchestrator.RunAsync(request, wifi, ct);

    Console.WriteLine();
    Console.WriteLine("=== result ===");
    Console.WriteLine($"status     : {result.Status}");
    Console.WriteLine($"user_id    : {result.UserId ?? "-"}");
    Console.WriteLine($"device_id  : {result.DeviceId ?? "-"}");
    Console.WriteLine($"message    : {result.Message ?? "-"}");

    return result.Status == RegistrationStatus.Succeeded ? 0 : 1;
}
