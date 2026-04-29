using AipmRegister.Core.Wifi;
using Microsoft.Extensions.DependencyInjection;

namespace AipmRegister.Hosting;

/// DI extensions that own the platform-specific wiring for AIPM Register.
/// Both the CLI and the GUI host call <see cref="AddAipmWifiPlatform"/>
/// instead of duplicating the #if WINDOWS / LINUX / MACOS block.
public static class ServiceCollectionExtensions
{
    /// Registers the Wi-Fi enumerator and adapter factory appropriate for
    /// the host platform. The chosen <see cref="IWifiAdapter"/> is then
    /// produced via <see cref="IWifiAdapterFactory.Create(WifiInterface)"/>
    /// after the caller has enumerated and picked an interface — replacing
    /// the older "register-IWifiAdapter-as-singleton" pattern that forced
    /// "first wireless interface wins".
    ///
    /// Falls back to no-op enumerator + factory when the build target is
    /// unsupported or the runtime OS does not match the build's platform
    /// define.
    public static IServiceCollection AddAipmWifiPlatform(this IServiceCollection services)
    {
#if WINDOWS
        if (OperatingSystem.IsWindows())
        {
            services.AddSingleton<IWifiInterfaceEnumerator, AipmRegister.Wifi.Windows.WindowsWifiInterfaceEnumerator>();
            services.AddSingleton<IWifiAdapterFactory,      AipmRegister.Wifi.Windows.WindowsWifiAdapterFactory>();
            return services;
        }
#elif LINUX
        if (OperatingSystem.IsLinux())
        {
            services.AddSingleton<IWifiInterfaceEnumerator, AipmRegister.Wifi.Linux.LinuxWifiInterfaceEnumerator>();
            services.AddSingleton<IWifiAdapterFactory,      AipmRegister.Wifi.Linux.LinuxWifiAdapterFactory>();
            return services;
        }
#elif MACOS
        if (OperatingSystem.IsMacOS())
        {
            services.AddSingleton<IWifiInterfaceEnumerator, AipmRegister.Wifi.MacOs.MacOsWifiInterfaceEnumerator>();
            services.AddSingleton<IWifiAdapterFactory,      AipmRegister.Wifi.MacOs.MacOsWifiAdapterFactory>();
            return services;
        }
#endif
        services.AddSingleton<IWifiInterfaceEnumerator, NoopWifiInterfaceEnumerator>();
        services.AddSingleton<IWifiAdapterFactory,      NoopWifiAdapterFactory>();
        return services;
    }

    /// Transitional helper used by hosts whose ViewModels (or other
    /// services) still inject <see cref="IWifiAdapter"/> directly.
    /// Resolves the factory's adapter for the first enumerated interface
    /// — preserving the legacy "first wireless wins" behavior — and
    /// caches the singleton. Once GUI's WifiPicker step uses
    /// <see cref="IWifiAdapterFactory"/> to honour the user's pick this
    /// shim becomes unnecessary.
    public static IServiceCollection AddAipmDefaultWifiAdapterShim(this IServiceCollection services)
    {
        services.AddSingleton<IWifiAdapter>(sp =>
        {
            var enumerator = sp.GetRequiredService<IWifiInterfaceEnumerator>();
            var factory    = sp.GetRequiredService<IWifiAdapterFactory>();
            var ifaces = enumerator.EnumerateAsync().GetAwaiter().GetResult();
            var iface  = ifaces.Count > 0
                ? ifaces[0]
                : new WifiInterface(Id: "noop", DisplayName: "No Wi-Fi adapter");
            return factory.Create(iface);
        });
        return services;
    }
}
