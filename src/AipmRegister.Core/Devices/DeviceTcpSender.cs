using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using AipmRegister.Core.Models;

namespace AipmRegister.Core.Devices;

public sealed class DeviceTcpSender : IDeviceTcpSender
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
    };

    public async Task<string> SendSettingsAsync(
        string deviceHost,
        int port,
        DeviceSettings settings,
        CancellationToken ct = default)
    {
        using var client = new TcpClient { NoDelay = true };
        await client.ConnectAsync(deviceHost, port, ct);

        await using var stream = client.GetStream();
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        var payload = Encoding.UTF8.GetBytes(json);
        await stream.WriteAsync(payload, ct);
        await stream.FlushAsync(ct);

        var buffer = new byte[1024];
        var read = await stream.ReadAsync(buffer, ct);
        return read > 0 ? Encoding.UTF8.GetString(buffer, 0, read) : string.Empty;
    }
}
