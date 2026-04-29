namespace AipmRegister.Core.Wifi;

/// Enumerates the wireless network interfaces available on the host. Each
/// platform implementation (Windows / Linux / macOS) returns a list — empty
/// when no Wi-Fi hardware is present, single-element on the common
/// laptop-with-one-radio case, multi-element when an external adapter or
/// virtualised radio is also attached.
///
/// The enumeration result is consumed by the CLI (which auto-picks when
/// the list has one entry, otherwise asks the user to pass
/// <c>--wifi-interface &lt;id&gt;</c>) and by the GUI's WifiPicker step
/// (which auto-hides its dropdown when <c>Count == 1</c>).
public interface IWifiInterfaceEnumerator
{
    Task<IReadOnlyList<WifiInterface>> EnumerateAsync(CancellationToken ct = default);
}
