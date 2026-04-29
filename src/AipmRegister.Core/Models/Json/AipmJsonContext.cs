using System.Text.Json.Serialization;

namespace AipmRegister.Core.Models.Json;

/// Source-generated System.Text.Json context covering every DTO this project
/// (de)serializes. Required for Native AOT — runtime reflection-based
/// serialization is trim-unfriendly, but the generator emits a static
/// JsonTypeInfo for each registered type so the JIT-less binary stays slim.
[JsonSerializable(typeof(GetPcKeyRequest))]
[JsonSerializable(typeof(GetPcKeyResponse))]
[JsonSerializable(typeof(Account))]
[JsonSerializable(typeof(ControlCheckRequest))]
[JsonSerializable(typeof(DeviceSettings))]
[JsonSourceGenerationOptions(
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
public sealed partial class AipmJsonContext : JsonSerializerContext
{
}
