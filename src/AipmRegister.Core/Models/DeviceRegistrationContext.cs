namespace AipmRegister.Core.Models;

/// Concrete identity of a device after the TCP-5000 hand-off completes.
/// Mirrors the (mac, model, deviceId) tuple frmMain assembled at line 1863.
public sealed record DeviceModelInfo(string Mac, string Model, string DeviceId);

/// One iteration of the v1/devices/control/check polling loop, surfaced to
/// the wizard so it can update the ProgressBar tick by tick.
public sealed record ControlCheckTick(
    int Attempt,
    int MaxAttempts,
    Models.ControlCheckOutcome Outcome,
    string RawResponse);
