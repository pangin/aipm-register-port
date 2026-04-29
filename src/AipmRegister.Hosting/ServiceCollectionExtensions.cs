using AipmRegister.Core.Process;
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
        services.AddSingleton<IProcessRunner, DefaultProcessRunner>();

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
}
