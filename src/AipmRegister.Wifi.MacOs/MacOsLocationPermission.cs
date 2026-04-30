using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;

namespace AipmRegister.Wifi.MacOs;

[SupportedOSPlatform("macos")]
internal static class MacOsLocationPermission
{
    private const string CoreLocationPath = "/System/Library/Frameworks/CoreLocation.framework/CoreLocation";
    private const int RtldNow = 0x2;

    private static readonly IntPtr AuthorizationStatusSelector = sel_registerName("authorizationStatus");
    private static readonly IntPtr LocationServicesEnabledSelector = sel_registerName("locationServicesEnabled");
    private static readonly IntPtr AllocSelector = sel_registerName("alloc");
    private static readonly IntPtr InitSelector = sel_registerName("init");
    private static readonly IntPtr RequestWhenInUseAuthorizationSelector = sel_registerName("requestWhenInUseAuthorization");

    private static IntPtr s_locationManager;

    public static void RequestWhenInUseAuthorizationForBundledApp(ILogger logger)
    {
        if (!OperatingSystem.IsMacOS() || !HasBundledLocationUsageDescription()) return;

        try
        {
            if (dlopen(CoreLocationPath, RtldNow) == IntPtr.Zero) return;

            var managerClass = objc_getClass("CLLocationManager");
            if (managerClass == IntPtr.Zero) return;

            if (objc_msgSend_bool(managerClass, LocationServicesEnabledSelector) == 0)
            {
                throw new InvalidOperationException(
                    "Location Services is off. Turn it on in System Settings > Privacy & Security > Location Services, then refresh Wi-Fi.");
            }

            var status = (int)objc_msgSend_long(managerClass, AuthorizationStatusSelector);
            switch (status)
            {
                case 0: // kCLAuthorizationStatusNotDetermined
                    s_locationManager = objc_msgSend_IntPtr(
                        objc_msgSend_IntPtr(managerClass, AllocSelector),
                        InitSelector);
                    objc_msgSend_void(s_locationManager, RequestWhenInUseAuthorizationSelector);
                    throw new InvalidOperationException(
                        "macOS needs Location Services permission before it can reveal Wi-Fi SSIDs. Approve the prompt for AipmRegister, then refresh Wi-Fi again.");

                case 1: // kCLAuthorizationStatusRestricted
                case 2: // kCLAuthorizationStatusDenied
                    throw new InvalidOperationException(
                        "AipmRegister does not have Location Services permission. Enable it in System Settings > Privacy & Security > Location Services, then refresh Wi-Fi.");
            }
        }
        catch (DllNotFoundException ex)
        {
            logger.LogDebug(ex, "CoreLocation is not available; Wi-Fi SSIDs may be redacted.");
        }
        catch (EntryPointNotFoundException ex)
        {
            logger.LogDebug(ex, "Objective-C runtime entry point is not available; Wi-Fi SSIDs may be redacted.");
        }
    }

    public static bool IsRedactedSsid(string ssid)
        => string.Equals(ssid, "<redacted>", StringComparison.OrdinalIgnoreCase);

    public static InvalidOperationException CreateRedactedSsidException()
        => new(
            "macOS returned redacted Wi-Fi SSIDs. Grant Location Services permission to AipmRegister in System Settings > Privacy & Security > Location Services, then refresh Wi-Fi.");

    private static bool HasBundledLocationUsageDescription()
    {
        var macOsDir = new DirectoryInfo(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar));
        if (!string.Equals(macOsDir.Name, "MacOS", StringComparison.Ordinal)) return false;

        var contentsDir = macOsDir.Parent;
        var appDir = contentsDir?.Parent;
        if (contentsDir is null
            || appDir is null
            || !string.Equals(contentsDir.Name, "Contents", StringComparison.Ordinal)
            || !appDir.Name.EndsWith(".app", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var infoPlist = Path.Combine(contentsDir.FullName, "Info.plist");
        if (!File.Exists(infoPlist)) return false;

        var contents = File.ReadAllText(infoPlist);
        return contents.Contains("NSLocationUsageDescription", StringComparison.Ordinal)
               || contents.Contains("NSLocationWhenInUseUsageDescription", StringComparison.Ordinal);
    }

    [DllImport("/usr/lib/libSystem.B.dylib")]
    private static extern IntPtr dlopen(string path, int mode);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr sel_registerName(string name);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern long objc_msgSend_long(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern byte objc_msgSend_bool(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_void(IntPtr receiver, IntPtr selector);
}
