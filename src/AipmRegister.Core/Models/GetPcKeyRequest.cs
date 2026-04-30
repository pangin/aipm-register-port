using System.Text.Json.Serialization;

namespace AipmRegister.Core.Models;

public sealed record GetPcKeyRequest(
    [property: JsonPropertyName("account")] GetPcKeyAccount Account);

public sealed record GetPcKeyAccount(
    [property: JsonPropertyName("pc_temp_key")] string PcTempKey);

public sealed record GetPcKeyResponse(
    [property: JsonPropertyName("account")] Account Account);
