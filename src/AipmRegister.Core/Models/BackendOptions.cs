namespace AipmRegister.Core.Models;

/// Hard-coded constants from the original (frmMain.cs lines 29~41) lifted
/// into a record so they can be overridden via configuration / DI in tests.
public sealed record BackendOptions
{
    public string ApiHost     { get; init; } = "dwapi.dawonai.com";
    public int    ApiPort     { get; init; } = 18443;
    public string MqttHost    { get; init; } = "dwmqtt.dawonai.com";
    public int    MqttPort    { get; init; } = 8883;
    public string SslSupport  { get; init; } = "yes";
    public string Company     { get; init; } = "DAWONDNS";
    public string Topic       { get; init; } = "dwd";
    public string HitVersion  { get; init; } = "1.0";

    public Uri ApiBase => new($"https://{ApiHost}:{ApiPort}/api/");
}
