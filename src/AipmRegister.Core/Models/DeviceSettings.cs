using System.Text.Json.Serialization;

namespace AipmRegister.Core.Models;

/// Settings payload pushed to the IoT device over TCP :5000 once we are
/// connected to its hotspot AP. Once the device receives this, it joins the
/// home Wi-Fi (using ssid/pass) and registers itself to the MQTT broker.
public sealed record DeviceSettings(
    [property: JsonPropertyName("mac")]              string Mac,
    [property: JsonPropertyName("api_server_addr")]  string ApiServerAddress,
    [property: JsonPropertyName("api_server_port")]  string ApiServerPort,
    [property: JsonPropertyName("server_addr")]      string MqttServerAddress,
    [property: JsonPropertyName("server_port")]      string MqttServerPort,
    [property: JsonPropertyName("ssl_support")]      string SslSupport,
    [property: JsonPropertyName("ssid")]             string HomeSsid,
    [property: JsonPropertyName("pass")]             string HomePassword,
    [property: JsonPropertyName("user_id")]          string UserId,
    [property: JsonPropertyName("company")]          string Company,
    [property: JsonPropertyName("model")]            string Model,
    [property: JsonPropertyName("lati")]             string Latitude,
    [property: JsonPropertyName("long")]             string Longitude,
    [property: JsonPropertyName("topic")]            string Topic);
