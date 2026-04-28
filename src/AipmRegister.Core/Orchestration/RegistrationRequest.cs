namespace AipmRegister.Core.Orchestration;

public sealed record RegistrationRequest(
    string AuthCode8Digits,
    string HomeSsid,
    string HomePassword,
    string DeviceHotspotSsid,
    string DeviceHotspotPassword,
    string DeviceTcpHost = "192.168.4.1",
    int    DeviceTcpPort = 5000,
    int    MaxControlCheckAttempts = 30,
    TimeSpan? PollInterval = null);

public enum RegistrationStatus
{
    Succeeded,
    AuthCodeInvalidOrExpired,
    DeviceTcpFailed,
    AlreadyRegistered,
    DeviceNotResponding,
    Cancelled,
}

public sealed record RegistrationResult(
    RegistrationStatus Status,
    string? UserId,
    string? DeviceId,
    string? Message);
