using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using AipmRegister.Core.Wifi;

namespace AipmRegister.Wifi.MacOs;

[SupportedOSPlatform("macos")]
internal static class CoreWlanScanner
{
    private const string CoreWlanPath = "/System/Library/Frameworks/CoreWLAN.framework/CoreWLAN";
    private const int RtldNow = 0x2;

    private static readonly IntPtr SharedWifiClientSelector       = sel_registerName("sharedWiFiClient");
    private static readonly IntPtr InterfaceSelector              = sel_registerName("interface");
    private static readonly IntPtr InterfaceWithNameSelector      = sel_registerName("interfaceWithName:");
    private static readonly IntPtr StringWithUtf8StringSelector   = sel_registerName("stringWithUTF8String:");
    private static readonly IntPtr ScanForNetworksWithNameSelector= sel_registerName("scanForNetworksWithName:error:");
    private static readonly IntPtr AllObjectsSelector             = sel_registerName("allObjects");
    private static readonly IntPtr CountSelector                  = sel_registerName("count");
    private static readonly IntPtr ObjectAtIndexSelector          = sel_registerName("objectAtIndex:");
    private static readonly IntPtr SsidSelector                   = sel_registerName("ssid");
    private static readonly IntPtr Utf8StringSelector             = sel_registerName("UTF8String");
    private static readonly IntPtr RssiValueSelector              = sel_registerName("rssiValue");
    private static readonly IntPtr WlanChannelSelector            = sel_registerName("wlanChannel");
    private static readonly IntPtr ChannelBandSelector            = sel_registerName("channelBand");
    private static readonly IntPtr ChannelNumberSelector          = sel_registerName("channelNumber");
    private static readonly IntPtr SupportsSecuritySelector       = sel_registerName("supportsSecurity:");
    private static readonly IntPtr LocalizedDescriptionSelector   = sel_registerName("localizedDescription");

    public static IReadOnlyList<WifiNetwork> Scan(string interfaceName)
    {
        var pool = objc_autoreleasePoolPush();
        try
        {
            if (dlopen(CoreWlanPath, RtldNow) == IntPtr.Zero)
            {
                throw new InvalidOperationException("CoreWLAN.framework is not available on this Mac.");
            }

            var clientClass = objc_getClass("CWWiFiClient");
            if (clientClass == IntPtr.Zero)
            {
                throw new InvalidOperationException("CoreWLAN CWWiFiClient class is not available on this Mac.");
            }

            var client = objc_msgSend_IntPtr(clientClass, SharedWifiClientSelector);
            var iface = GetInterface(client, interfaceName);
            if (iface == IntPtr.Zero)
            {
                throw new InvalidOperationException($"Wi-Fi interface '{interfaceName}' was not found.");
            }

            var error = IntPtr.Zero;
            var networksSet = objc_msgSend_IntPtr_IntPtr_OutIntPtr(
                iface,
                ScanForNetworksWithNameSelector,
                IntPtr.Zero,
                out error);

            if (networksSet == IntPtr.Zero)
            {
                throw new InvalidOperationException($"CoreWLAN scan failed: {NSErrorDescription(error)}");
            }

            var networks = ToWifiNetworks(networksSet);
            if (networks.Count == 0 && objc_msgSend_nuint(networksSet, CountSelector) > 0)
            {
                throw new InvalidOperationException(
                    "CoreWLAN scanned nearby Wi-Fi networks, but macOS hid every SSID. Grant Location Services permission to AipmRegister, then refresh Wi-Fi.");
            }

            return networks;
        }
        finally
        {
            objc_autoreleasePoolPop(pool);
        }
    }

    private static IntPtr GetInterface(IntPtr client, string interfaceName)
    {
        if (!string.IsNullOrWhiteSpace(interfaceName))
        {
            var name = NSString(interfaceName);
            var iface = objc_msgSend_IntPtr_IntPtr(client, InterfaceWithNameSelector, name);
            if (iface != IntPtr.Zero) return iface;
        }

        return objc_msgSend_IntPtr(client, InterfaceSelector);
    }

    private static List<WifiNetwork> ToWifiNetworks(IntPtr networksSet)
    {
        var array = objc_msgSend_IntPtr(networksSet, AllObjectsSelector);
        var count = objc_msgSend_nuint(array, CountSelector);
        var results = new List<WifiNetwork>();
        var seen = new HashSet<(string Ssid, string Band, WifiSecurity Security)>();

        for (nuint i = 0; i < count; i++)
        {
            var network = objc_msgSend_IntPtr_nuint(array, ObjectAtIndexSelector, i);
            var ssid = NSStringToString(objc_msgSend_IntPtr(network, SsidSelector));
            if (string.IsNullOrWhiteSpace(ssid)) continue;

            var security = GetSecurity(network);
            var band = GetBand(network);
            if (!seen.Add((ssid, band, security))) continue;

            results.Add(new WifiNetwork(
                Ssid:          ssid,
                SignalQuality: RssiToQuality((int)objc_msgSend_nint(network, RssiValueSelector)),
                Security:      security,
                Band:          band));
        }

        return results;
    }

    private static WifiSecurity GetSecurity(IntPtr network)
    {
        if (SupportsSecurity(network, 4) || SupportsSecurity(network, 5)) return WifiSecurity.Wpa2Personal;
        if (SupportsSecurity(network, 11) || SupportsSecurity(network, 13)) return WifiSecurity.Wpa3Personal;
        if (SupportsSecurity(network, 2) || SupportsSecurity(network, 3)) return WifiSecurity.WpaPersonal;
        if (SupportsSecurity(network, 1)) return WifiSecurity.Wep;
        return WifiSecurity.Open;
    }

    private static bool SupportsSecurity(IntPtr network, nint security)
        => objc_msgSend_bool_nint(network, SupportsSecuritySelector, security) != 0;

    private static string GetBand(IntPtr network)
    {
        var channel = objc_msgSend_IntPtr(network, WlanChannelSelector);
        if (channel == IntPtr.Zero) return WifiBandClassifier.Unknown;

        return objc_msgSend_nint(channel, ChannelBandSelector) switch
        {
            1 => WifiBandClassifier.Band24,
            2 => WifiBandClassifier.Band5,
            3 => WifiBandClassifier.Band6,
            _ => ChannelNumberToBand((int)objc_msgSend_nint(channel, ChannelNumberSelector)),
        };
    }

    private static string ChannelNumberToBand(int channel)
        => WifiBandClassifier.FromChannel(channel.ToString(System.Globalization.CultureInfo.InvariantCulture));

    private static int RssiToQuality(int dbm)
    {
        if (dbm >= -50) return 100;
        if (dbm <= -100) return 0;
        return 2 * (dbm + 100);
    }

    private static string NSErrorDescription(IntPtr error)
        => error == IntPtr.Zero
            ? "unknown error"
            : NSStringToString(objc_msgSend_IntPtr(error, LocalizedDescriptionSelector)) ?? "unknown error";

    private static IntPtr NSString(string value)
    {
        var nsStringClass = objc_getClass("NSString");
        var utf8 = Marshal.StringToCoTaskMemUTF8(value);
        try
        {
            return objc_msgSend_IntPtr_IntPtr(nsStringClass, StringWithUtf8StringSelector, utf8);
        }
        finally
        {
            Marshal.FreeCoTaskMem(utf8);
        }
    }

    private static string? NSStringToString(IntPtr nsString)
    {
        if (nsString == IntPtr.Zero) return null;
        var utf8 = objc_msgSend_IntPtr(nsString, Utf8StringSelector);
        return utf8 == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(utf8);
    }

    [DllImport("/usr/lib/libSystem.B.dylib")]
    private static extern IntPtr dlopen(string path, int mode);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr sel_registerName(string name);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_autoreleasePoolPush();

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern void objc_autoreleasePoolPop(IntPtr pool);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_IntPtr_IntPtr(IntPtr receiver, IntPtr selector, IntPtr value);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_IntPtr_IntPtr_OutIntPtr(
        IntPtr receiver,
        IntPtr selector,
        IntPtr value,
        out IntPtr error);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_IntPtr_nuint(IntPtr receiver, IntPtr selector, nuint index);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern nint objc_msgSend_nint(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern nuint objc_msgSend_nuint(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern byte objc_msgSend_bool_nint(IntPtr receiver, IntPtr selector, nint value);
}
