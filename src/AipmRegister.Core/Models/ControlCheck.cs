using System.Text.Json.Serialization;

namespace AipmRegister.Core.Models;

public sealed record ControlCheckRequest(
    ControlCheckAccount Account,
    IReadOnlyList<ControlCheckDevice> Devices);

public sealed record ControlCheckAccount(
    [property: JsonPropertyName("user_id")] string UserId,
    [property: JsonPropertyName("pc_key")]  string PcKey);

public sealed record ControlCheckDevice(
    [property: JsonPropertyName("device_id")] string DeviceId,
    ControlCheckMessage Msg);

public sealed record ControlCheckMessage(
    [property: JsonPropertyName("e")] IReadOnlyList<ControlCheckEntry> Entries,
    [property: JsonPropertyName("o")] string Operation);

public sealed record ControlCheckEntry(
    [property: JsonPropertyName("n")] string Name);

public enum ControlCheckOutcome
{
    Pending,
    Success,
    AlreadyRegistered,
    NotRegisteredExceededAttempts,
    AuthCodeExpired,
    UnknownError,
}
