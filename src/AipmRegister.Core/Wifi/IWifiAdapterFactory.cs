namespace AipmRegister.Core.Wifi;

/// Produces the <see cref="IWifiAdapter"/> instance bound to a chosen
/// <see cref="WifiInterface"/>. Replaces the older pattern where each
/// platform adapter was a singleton resolved at app-start (which forced
/// "first wireless interface wins"); the factory now defers the choice
/// until after enumeration + user selection, while caching one adapter
/// instance per interface id so repeated calls return the same object
/// (e.g. WifiPicker → DevicePicker → Registering all share the chosen
/// adapter).
public interface IWifiAdapterFactory
{
    IWifiAdapter Create(WifiInterface iface);
}
