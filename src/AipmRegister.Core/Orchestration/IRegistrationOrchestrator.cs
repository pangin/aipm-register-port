using AipmRegister.Core.Models;
using AipmRegister.Core.Wifi;

namespace AipmRegister.Core.Orchestration;

/// Top-level API consumed by both the CLI and the Avalonia GUI. Drives the
/// full register-a-device pipeline that frmMain.cs spread across timers,
/// async callbacks, and tab indices.
///
/// The wizard (Phase K) drives steps 2-5 individually via the smaller hooks
/// below; the all-in-one RunAsync remains for the CLI's headless flow.
public interface IRegistrationOrchestrator
{
    /// Runs every step end-to-end (CLI flow, also used by tests). The
    /// caller resolves the chosen <see cref="IWifiAdapter"/> via
    /// <see cref="IWifiAdapterFactory"/> after enumerating + picking an
    /// interface, then hands it in here.
    Task<RegistrationResult> RunAsync(
        RegistrationRequest request,
        IWifiAdapter wifi,
        CancellationToken ct = default);

    /// Step 2/5 — POST v1/accounts/post/getPckey, parse the Account.
    Task<Account?> ExchangeAuthCodeAsync(
        string authCode8Digits,
        CancellationToken ct = default);

    /// Step 4/5 finale — push DeviceSettings JSON over TCP, derive the
    /// deviceId from what the device replies. Caller is responsible for
    /// already being connected to the device hotspot.
    Task<DeviceModelInfo> SendDeviceSettingsAsync(
        Account account,
        ProductDefinition picked,
        string deviceHotspotSsid,
        string homeSsid,
        string homePassword,
        string deviceTcpHost,
        int deviceTcpPort,
        CancellationToken ct = default);

    /// End-to-end Wi-Fi hand-off used by the wizard's "Registering" step
    /// (and re-used by <see cref="RunAsync"/>): join the device hotspot,
    /// push settings, leave the hotspot, rejoin the home network. Returns
    /// the <see cref="DeviceModelInfo"/> derived from the device's TCP
    /// reply.
    Task<DeviceModelInfo> HandOffToDeviceAsync(
        Account account,
        ProductDefinition picked,
        RegistrationRequest request,
        IWifiAdapter wifi,
        CancellationToken ct = default);

    /// Step 5/5 — keeps polling control/check, yielding one tick per call
    /// so the wizard can advance its ProgressBar in lock-step. The stream
    /// ends on the first terminal outcome (Success / AlreadyRegistered /
    /// AuthCodeExpired / NotRegistered) or after maxAttempts.
    IAsyncEnumerable<ControlCheckTick> PollRegistrationAsync(
        Account account,
        string deviceId,
        int maxAttempts,
        TimeSpan pollInterval,
        CancellationToken ct = default);
}
