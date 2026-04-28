namespace AipmRegister.Core.Orchestration;

/// Top-level API consumed by both the CLI and the Avalonia GUI. Drives the
/// full register-a-device pipeline that frmMain.cs spread across timers,
/// async callbacks, and tab indices.
public interface IRegistrationOrchestrator
{
    Task<RegistrationResult> RunAsync(
        RegistrationRequest request,
        CancellationToken ct = default);
}
