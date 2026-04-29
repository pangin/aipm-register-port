namespace AipmRegister.Core.Wifi;

/// A single wireless network interface available on the host. The minimal
/// fields obtainable on Windows / Linux / macOS without falling back to
/// platform-specific tooling that may not be installed.
///
/// <see cref="Id"/> is the platform-stable handle the user passes back to
/// re-acquire the same adapter (interface name on Linux/macOS, GUID on
/// Windows). <see cref="DisplayName"/> is what the UI shows; on Linux this
/// is typically equal to <see cref="Id"/> because the kernel doesn't
/// expose a friendly name without nmcli/udevadm.
public sealed record WifiInterface(string Id, string DisplayName, string? Description = null);
