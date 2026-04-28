using System.Text.Json.Serialization;

namespace AipmRegister.Core.Models;

public sealed record Account(
    [property: JsonPropertyName("user_id")] string UserId,
    [property: JsonPropertyName("pc_key")]  string PcKey,
    [property: JsonPropertyName("pc_lati")] string Latitude,
    [property: JsonPropertyName("pc_long")] string Longitude);
