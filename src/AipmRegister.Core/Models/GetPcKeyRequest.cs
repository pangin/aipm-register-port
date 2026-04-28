using System.Text.Json.Serialization;

namespace AipmRegister.Core.Models;

public sealed record GetPcKeyRequest(GetPcKeyAccount Account);

public sealed record GetPcKeyAccount(
    [property: JsonPropertyName("pc_temp_key")] string PcTempKey);

public sealed record GetPcKeyResponse(Account Account);
