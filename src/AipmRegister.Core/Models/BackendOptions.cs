namespace AipmRegister.Core.Models;

/// Hard-coded constants from the original (frmMain.cs lines 29~41) lifted
/// into a record so they can be overridden via configuration / DI in
/// tests. The defaults match the vendor's production cloud endpoints —
/// callers that need a different backend (staging, an internal stand-in,
/// a forked project) construct with named arguments:
///
/// <code>
/// services.AddSingleton(new BackendOptions(ApiHost: "test.api.example.com"));
/// </code>
public sealed record BackendOptions(
    string ApiHost    = "dwapi.dawonai.com",
    int    ApiPort    = 18443,
    string MqttHost   = "dwmqtt.dawonai.com",
    int    MqttPort   = 8883,
    string SslSupport = "yes",
    string Company    = "DAWONDNS",
    string Topic      = "dwd",
    string HitVersion = "1.0")
{
    /// HTTPS base URL composed from <see cref="ApiHost"/> and
    /// <see cref="ApiPort"/>. Throws if the host part is empty —
    /// ensures the malformed config surfaces at first use rather than
    /// failing deep inside HttpClient with an opaque message.
    public Uri ApiBase
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ApiHost))
            {
                throw new InvalidOperationException(
                    $"{nameof(BackendOptions)}.{nameof(ApiHost)} must be a non-empty hostname.");
            }
            return new Uri($"https://{ApiHost}:{ApiPort}/api/");
        }
    }
}
