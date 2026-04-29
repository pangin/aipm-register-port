namespace AipmRegister.Core.Notification;

/// Identifies which leg of the registration pipeline a
/// <see cref="IUserNotifier.Progress"/> event came from. Replaces the
/// previous magic-string parameter (`"auth"`, `"wifi"`, `"device"`,
/// `"poll"`) so callers can't typo the value and notifier
/// implementations can switch on a closed set.
public enum RegistrationStage
{
    /// Step 2/5 — exchanging the 8-digit auth code for an Account.
    Auth,

    /// Step 1/5 (home network) and step 4/5→5/5 transitions (device
    /// hotspot join + leave). Anything that toggles a Wi-Fi association.
    Wifi,

    /// Step 4/5 — pushing DeviceSettings JSON to the device over TCP.
    Device,

    /// Step 5/5 — polling cloud's control/check until a terminal outcome.
    Poll,

    /// Pipeline completed (success or final failure).
    Done,
}
