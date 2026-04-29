using AipmRegister.Core.Wifi;
using Microsoft.Extensions.DependencyInjection;

namespace AipmRegister.Hosting;

/// DI extensions that own the platform-specific wiring for AIPM Register.
/// Both the CLI and the GUI host call <see cref="AddAipmWifiPlatform"/>
/// instead of duplicating the #if WINDOWS / LINUX / MACOS block.
public static class ServiceCollectionExtensions
{
    /// Registers the Wi-Fi adapter implementation appropriate for the host
    /// platform. Falls back to <see cref="NoopWifiAdapter"/> when the build
    /// target is unsupported or when the runtime OS does not match the
    /// build's platform define.
    public static IServiceCollection AddAipmWifiPlatform(this IServiceCollection services)
    {
#if WINDOWS
        if (OperatingSystem.IsWindows())
        {
            services.AddSingleton<IWifiAdapter, AipmRegister.Wifi.Windows.WindowsWifiAdapter>();
        }
        else
        {
            services.AddSingleton<IWifiAdapter, NoopWifiAdapter>();
        }
#elif LINUX
        if (OperatingSystem.IsLinux())
        {
            services.AddSingleton<IWifiAdapter, AipmRegister.Wifi.Linux.LinuxWifiAdapter>();
        }
        else
        {
            services.AddSingleton<IWifiAdapter, NoopWifiAdapter>();
        }
#elif MACOS
        if (OperatingSystem.IsMacOS())
        {
            services.AddSingleton<IWifiAdapter, AipmRegister.Wifi.MacOs.MacOsWifiAdapter>();
        }
        else
        {
            services.AddSingleton<IWifiAdapter, NoopWifiAdapter>();
        }
#else
        services.AddSingleton<IWifiAdapter, NoopWifiAdapter>();
#endif
        return services;
    }
}
