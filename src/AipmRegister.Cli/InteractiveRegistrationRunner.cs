using AipmRegister.Core.Models;
using AipmRegister.Core.Network;
using AipmRegister.Core.Orchestration;
using AipmRegister.Core.Wifi;
using Microsoft.Extensions.DependencyInjection;

namespace AipmRegister.Cli;

internal static class InteractiveRegistrationRunner
{
    public static async Task<int> RunAsync(
        IServiceProvider services,
        CliRegistrationOptions initial,
        CancellationToken ct)
    {
        var orchestrator = services.GetRequiredService<IRegistrationOrchestrator>();
        var enumerator = services.GetRequiredService<IWifiInterfaceEnumerator>();
        var factory = services.GetRequiredService<IWifiAdapterFactory>();
        var backend = services.GetRequiredService<BackendOptions>();
        var reachability = services.GetRequiredService<IInternetReachabilityProbe>();

        WriteTitle("AIPM Register interactive");
        Console.WriteLine("GUI와 같은 순서로 진행하면서 각 단계의 식별자를 확인합니다.");
        Console.WriteLine("중간에 Ctrl+C를 누르면 중단됩니다.");
        Console.WriteLine();

        var iface = await PickInterfaceAsync(enumerator, initial.WifiInterface, ct);
        if (iface is null) return 2;
        var wifi = factory.Create(iface);

        var home = await PickHomeWifiAsync(wifi, initial, ct);
        if (home is null) return 2;

        WriteTitle("1/5 home Wi-Fi");
        await wifi.ConnectAsync(home.Value.Ssid, home.Value.Password, home.Value.Security, ct);
        await reachability.WaitUntilReachableAsync(backend.ApiHost, backend.ApiPort, TimeSpan.FromSeconds(30), ct);
        WriteOk($"internet reachable: {backend.ApiHost}:{backend.ApiPort}");

        WriteTitle("2/5 auth");
        var authCode = ReadRequired("8-digit auth code", initial.AuthCode);
        var account = await orchestrator.ExchangeAuthCodeAsync(authCode, ct);
        if (account is null)
        {
            WriteErr("auth failed: code is invalid or expired");
            return 1;
        }
        WriteOk($"user_id={account.UserId}, pc_key={Mask(account.PcKey)}, lat={account.Latitude}, long={account.Longitude}");

        WriteTitle("3/5 product");
        var product = PickProduct();
        WriteOk($"product={product.Tag}, primary={product.PrimaryPrefix}, secondary={Blank(product.SecondaryPrefix)}");

        WriteTitle("4/5 device hotspot");
        var deviceSsid = await PickDeviceHotspotAsync(wifi, product, initial.DeviceHotspotSsid, ct);
        if (string.IsNullOrWhiteSpace(deviceSsid)) return 2;

        var mac = HotspotSsidParser.ExtractMac(deviceSsid);
        var model = ProductCatalog.ResolveModelCode(deviceSsid, product);
        var expectedDeviceId = $"{backend.Company}-{model}-{mac}";
        Console.WriteLine($"ssid       : {deviceSsid}");
        Console.WriteLine($"mac        : {mac}");
        Console.WriteLine($"model      : {model}");
        Console.WriteLine($"device_id  : {expectedDeviceId}");
        Console.WriteLine($"tcp target : {initial.DeviceHost}:{initial.DevicePort} (gateway auto-detect can override this)");
        Console.WriteLine();
        PromptEnter("Press Enter to hand off settings to the device...");

        var request = new RegistrationRequest(
            AuthCode8Digits: authCode,
            HomeSsid: home.Value.Ssid,
            HomePassword: home.Value.Password,
            DeviceHotspotSsid: deviceSsid,
            DeviceHotspotPassword: initial.DeviceHotspotPassword,
            DeviceTcpHost: initial.DeviceHost,
            DeviceTcpPort: initial.DevicePort,
            MaxControlCheckAttempts: initial.MaxAttempts,
            PollInterval: initial.PollInterval,
            HomeSecurity: home.Value.Security);

        WriteTitle("5/5 handoff");
        DeviceModelInfo info;
        try
        {
            info = await orchestrator.HandOffToDeviceAsync(account, product, request, wifi, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            WriteErr($"handoff failed: {ex.Message}");
            return 1;
        }

        Console.WriteLine($"computed device_id : {info.DeviceId}");
        if (!string.Equals(info.DeviceId, expectedDeviceId, StringComparison.Ordinal))
        {
            WriteWarn($"device_id mismatch, expected {expectedDeviceId}");
        }

        WriteTitle("control/check poll");
        ControlCheckTick? terminal = null;
        await foreach (var tick in orchestrator.PollRegistrationAsync(
                           account,
                           info.DeviceId,
                           initial.MaxAttempts,
                           initial.PollInterval,
                           ct))
        {
            terminal = tick;
            Console.WriteLine($"[{tick.Attempt,2}/{tick.MaxAttempts}] outcome={tick.Outcome}");
            if (!string.IsNullOrWhiteSpace(tick.RawResponse))
            {
                Console.WriteLine($"raw: {tick.RawResponse}");
            }
            if (tick.Outcome is ControlCheckOutcome.Success
                or ControlCheckOutcome.AlreadyRegistered
                or ControlCheckOutcome.AuthCodeExpired)
            {
                break;
            }
        }

        Console.WriteLine();
        Console.WriteLine("=== result ===");
        Console.WriteLine($"user_id    : {account.UserId}");
        Console.WriteLine($"device_id  : {info.DeviceId}");
        Console.WriteLine($"terminal   : {terminal?.Outcome.ToString() ?? "TimedOut"}");

        return terminal?.Outcome == ControlCheckOutcome.Success ? 0 : 1;
    }

    public static async Task<WifiInterface?> PickInterfaceAsync(
        IWifiInterfaceEnumerator enumerator,
        string? requestedId,
        CancellationToken ct)
    {
        WriteTitle("Wi-Fi interface");
        var ifaces = await enumerator.EnumerateAsync(ct);
        if (ifaces.Count == 0)
        {
            WriteErr("No wireless interface found on this host.");
            return null;
        }

        if (!string.IsNullOrWhiteSpace(requestedId))
        {
            var requested = ifaces.FirstOrDefault(i => string.Equals(i.Id, requestedId, StringComparison.Ordinal));
            if (requested is not null)
            {
                WriteOk($"{requested.Id} - {requested.DisplayName}");
                return requested;
            }
            WriteWarn($"requested interface '{requestedId}' was not found; choose from the list.");
        }

        if (ifaces.Count == 1)
        {
            WriteOk($"{ifaces[0].Id} - {ifaces[0].DisplayName}");
            return ifaces[0];
        }

        return PickFromList(
            ifaces,
            i => $"{i.Id} - {i.DisplayName}{(string.IsNullOrWhiteSpace(i.Description) ? "" : $" ({i.Description})")}");
    }

    private static async Task<HomeWifiSelection?> PickHomeWifiAsync(
        IWifiAdapter wifi,
        CliRegistrationOptions initial,
        CancellationToken ct)
    {
        WriteTitle("Home Wi-Fi scan");
        IReadOnlyList<WifiNetwork> networks = Array.Empty<WifiNetwork>();
        try
        {
            networks = await wifi.ScanAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            WriteWarn($"scan failed: {ex.Message}");
        }

        WifiNetwork? selected = null;
        if (!string.IsNullOrWhiteSpace(initial.HomeSsid))
        {
            selected = networks
                .Where(n => string.Equals(n.Ssid, initial.HomeSsid, StringComparison.Ordinal))
                .OrderByDescending(n => n.IsRecommended)
                .ThenByDescending(n => n.SignalQuality)
                .FirstOrDefault();
        }

        selected ??= PickNetwork(
            networks
                .Where(n => !LooksLikeDeviceHotspot(n.Ssid))
                .OrderByDescending(n => n.IsRecommended)
                .ThenByDescending(n => n.SignalQuality)
                .ToList(),
            "Select home Wi-Fi, or type m for manual SSID");

        var ssid = selected?.Ssid ?? ReadRequired("home SSID", initial.HomeSsid);
        var security = selected?.Security ?? PickSecurity(WifiSecurity.Wpa2Personal);
        var password = security == WifiSecurity.Open
            ? string.Empty
            : initial.HomePassword ?? ReadSecret("home Wi-Fi password");

        Console.WriteLine($"home ssid : {ssid}");
        Console.WriteLine($"security  : {security}");
        return new HomeWifiSelection(ssid, password, security);
    }

    private static ProductDefinition PickProduct()
    {
        var products = ProductCatalog.All.ToList();
        return PickFromList(products, p =>
            $"{p.Tag,-7} primary={p.PrimaryPrefix,-11} secondary={Blank(p.SecondaryPrefix),-11} model={p.ModelCode}");
    }

    private static async Task<string?> PickDeviceHotspotAsync(
        IWifiAdapter wifi,
        ProductDefinition product,
        string? initialSsid,
        CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(initialSsid))
        {
            WriteOk($"using provided device hotspot: {initialSsid}");
            return initialSsid;
        }

        while (true)
        {
            IReadOnlyList<WifiNetwork> networks = Array.Empty<WifiNetwork>();
            try
            {
                networks = await wifi.ScanAsync(ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                WriteWarn($"device scan failed: {ex.Message}");
            }

            var candidates = networks
                .Where(n => product.IsHotspotOf(n.Ssid))
                .OrderByDescending(n => n.SignalQuality)
                .ToList();

            var selected = PickNetwork(candidates, "Select device hotspot, r to rescan, or m for manual SSID", allowRefresh: true);
            if (selected is not null) return selected.Ssid;

            var cmd = LastNetworkCommand;
            if (cmd == "r") continue;
            if (cmd == "m") return ReadRequired("device hotspot SSID", null);
        }
    }

    private static string LastNetworkCommand { get; set; } = string.Empty;

    private static WifiNetwork? PickNetwork(IReadOnlyList<WifiNetwork> networks, string prompt, bool allowRefresh = false)
    {
        LastNetworkCommand = string.Empty;
        if (networks.Count > 0)
        {
            for (var i = 0; i < networks.Count; i++)
            {
                var n = networks[i];
                var marker = n.IsRecommended ? "ok" : "warn";
                Console.WriteLine($"{i + 1,2}. [{marker}] {n.Ssid}  signal={n.SignalQuality}%  band={n.Band}  security={n.Security}");
            }
        }
        else
        {
            WriteWarn("no matching networks found");
        }

        while (true)
        {
            Console.Write($"{prompt}: ");
            var input = Console.ReadLine()?.Trim() ?? string.Empty;
            if (allowRefresh && input.Equals("r", StringComparison.OrdinalIgnoreCase))
            {
                LastNetworkCommand = "r";
                return null;
            }
            if (input.Equals("m", StringComparison.OrdinalIgnoreCase))
            {
                LastNetworkCommand = "m";
                return null;
            }
            if (int.TryParse(input, out var index) && index >= 1 && index <= networks.Count)
            {
                return networks[index - 1];
            }
            WriteWarn("Enter a valid number" + (allowRefresh ? ", r" : "") + ", or m.");
        }
    }

    private static WifiSecurity PickSecurity(WifiSecurity fallback)
    {
        var values = Enum.GetValues<WifiSecurity>();
        Console.WriteLine("Security:");
        for (var i = 0; i < values.Length; i++)
        {
            Console.WriteLine($"{i + 1}. {values[i]}");
        }

        while (true)
        {
            Console.Write($"Select security [{fallback}]: ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input)) return fallback;
            if (int.TryParse(input, out var index) && index >= 1 && index <= values.Length)
            {
                return values[index - 1];
            }
            WriteWarn("Enter a valid number.");
        }
    }

    private static T PickFromList<T>(IReadOnlyList<T> items, Func<T, string> describe)
    {
        for (var i = 0; i < items.Count; i++)
        {
            Console.WriteLine($"{i + 1,2}. {describe(items[i])}");
        }

        while (true)
        {
            Console.Write("Select number: ");
            var input = Console.ReadLine()?.Trim();
            if (int.TryParse(input, out var index) && index >= 1 && index <= items.Count)
            {
                return items[index - 1];
            }
            WriteWarn("Enter a valid number.");
        }
    }

    private static string ReadRequired(string label, string? defaultValue)
    {
        while (true)
        {
            Console.Write(defaultValue is null ? $"{label}: " : $"{label} [{defaultValue}]: ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input) && !string.IsNullOrWhiteSpace(defaultValue)) return defaultValue;
            if (!string.IsNullOrWhiteSpace(input)) return input.Trim();
            WriteWarn($"{label} is required.");
        }
    }

    private static string ReadSecret(string label)
    {
        Console.Write($"{label}: ");
        if (Console.IsInputRedirected)
        {
            return Console.ReadLine() ?? string.Empty;
        }

        var chars = new List<char>();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return new string(chars.ToArray());
            }
            if (key.Key == ConsoleKey.Backspace)
            {
                if (chars.Count > 0)
                {
                    chars.RemoveAt(chars.Count - 1);
                    Console.Write("\b \b");
                }
                continue;
            }
            if (!char.IsControl(key.KeyChar))
            {
                chars.Add(key.KeyChar);
                Console.Write('*');
            }
        }
    }

    private static void PromptEnter(string message)
    {
        Console.Write(message);
        Console.ReadLine();
    }

    private static bool LooksLikeDeviceHotspot(string ssid)
        => ssid.StartsWith("DWD-", StringComparison.Ordinal)
            || ssid.StartsWith("DAWON_IRBD_", StringComparison.Ordinal);

    private static string Blank(string value) => string.IsNullOrWhiteSpace(value) ? "-" : value;

    private static string Mask(string value)
        => value.Length <= 4 ? "****" : $"{value[..2]}...{value[^2..]}";

    private static void WriteTitle(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"=== {title} ===");
    }

    private static void WriteOk(string message) => Write(ConsoleColor.Green, "OK", message);
    private static void WriteWarn(string message) => Write(ConsoleColor.Yellow, "WARN", message);
    private static void WriteErr(string message) => Write(ConsoleColor.Red, "ERR", message);

    private static void Write(ConsoleColor color, string tag, string message)
    {
        var previous = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"{tag,-5} {message}");
        }
        finally
        {
            Console.ForegroundColor = previous;
        }
    }

    private readonly record struct HomeWifiSelection(string Ssid, string Password, WifiSecurity Security);
}
