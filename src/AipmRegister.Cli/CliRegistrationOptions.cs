using AipmRegister.Core.Wifi;

namespace AipmRegister.Cli;

internal sealed record CliRegistrationOptions(
    string? AuthCode,
    string? DeviceHotspotSsid,
    string? HomeSsid,
    string? HomePassword,
    string DeviceHotspotPassword,
    string DeviceHost,
    int DevicePort,
    int MaxAttempts,
    TimeSpan PollInterval,
    string? WifiInterface,
    WifiSecurity HomeSecurity,
    bool Verbose)
{
    public bool HasHeadlessRequiredFields =>
        !string.IsNullOrWhiteSpace(AuthCode)
        && !string.IsNullOrWhiteSpace(DeviceHotspotSsid)
        && !string.IsNullOrWhiteSpace(HomeSsid)
        && HomePassword is not null;
}
