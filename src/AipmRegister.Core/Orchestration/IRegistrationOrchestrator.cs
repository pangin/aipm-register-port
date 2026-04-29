using AipmRegister.Core.Models;

namespace AipmRegister.Core.Orchestration;

/// Top-level API consumed by both the CLI and the Avalonia GUI. Drives the
/// full register-a-device pipeline that frmMain.cs spread across timers,
/// async callbacks, and tab indices.
///
/// The wizard (Phase K) drives steps 2-5 individually via the smaller hooks
/// below; the all-in-one RunAsync remains for the CLI's headless flow.
public interface IRegistrationOrchestrator
{
    /// Runs every step end-to-end (CLI flow, also used by tests).
    Task<RegistrationResult> RunAsync(
        RegistrationRequest request,
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

    /// Step 5/5 — keeps polling control/check, yielding one tick per call
    /// so the wizard can advance its ProgressBar in lock-step. The stream
    /// ends on the first terminal outcome (Success / AlreadyRegistered /
    /// AuthCodeExpired / NotRegisteredExceededAttempts) or after maxAttempts.
    IAsyncEnumerable<ControlCheckTick> PollRegistrationAsync(
        Account account,
        string deviceId,
        int maxAttempts,
        TimeSpan pollInterval,
        CancellationToken ct = default);
}
