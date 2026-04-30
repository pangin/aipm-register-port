using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using AipmRegister.Core.Models;
using AipmRegister.Core.Models.Json;

namespace AipmRegister.Core.Devices;

public sealed class DeviceTcpSender : IDeviceTcpSender
{
    private static readonly TimeSpan ConnectRetryWindow = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan ConnectRetryDelay = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan ConnectAttemptTimeout = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan StartAckTimeout = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan ResponseReadTimeout = TimeSpan.FromSeconds(5);
    private const string StartCommand = "[DUT<-PC] START";

    public async Task<string> SendSettingsAsync(
        string deviceHost,
        int port,
        DeviceSettings settings,
        CancellationToken ct = default)
    {
        using var client = await ConnectWithRetryAsync(deviceHost, port, ct);

        await using var stream = client.GetStream();
        var json = JsonSerializer.Serialize(settings, AipmJsonContext.Default.DeviceSettings);
        await WriteLineAsync(stream, StartCommand, ct);
        await stream.FlushAsync(ct);

        var reply = await ReadUntilAsync(stream, ContainsStartAccepted, StartAckTimeout, ct);
        await WriteLineAsync(stream, json, ct);
        await stream.FlushAsync(ct);

        reply += await ReadUntilAsync(stream, ContainsSettingsAccepted, ResponseReadTimeout, ct);
        return reply;
    }

    private static async Task WriteLineAsync(NetworkStream stream, string line, CancellationToken ct)
    {
        var payload = Encoding.UTF8.GetBytes(line + "\r\n");
        await stream.WriteAsync(payload, ct);
    }

    private static async Task<string> ReadUntilAsync(
        NetworkStream stream,
        Func<string, bool> isTerminal,
        TimeSpan timeout,
        CancellationToken ct)
    {
        var buffer = new byte[1024];
        var reply = new StringBuilder();

        while (true)
        {
            int read;
            try
            {
                read = await stream
                    .ReadAsync(buffer.AsMemory(0, buffer.Length), ct)
                    .AsTask()
                    .WaitAsync(timeout, ct);
            }
            catch (TimeoutException)
            {
                return reply.ToString();
            }

            if (read <= 0) return reply.ToString();

            reply.Append(Encoding.UTF8.GetString(buffer, 0, read));
            if (isTerminal(reply.ToString())) return reply.ToString();
        }
    }

    private static bool ContainsStartAccepted(string reply)
        => reply.Contains("START_OK", StringComparison.OrdinalIgnoreCase);

    private static bool ContainsSettingsAccepted(string reply)
    {
        var compact = reply.Replace(" ", string.Empty);
        return compact.Contains("\"respone\":\"OK\"", StringComparison.OrdinalIgnoreCase)
            || compact.Contains("\"response\":\"OK\"", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<TcpClient> ConnectWithRetryAsync(string deviceHost, int port, CancellationToken ct)
    {
        var deadline = DateTime.UtcNow + ConnectRetryWindow;
        Exception? lastError = null;

        while (DateTime.UtcNow < deadline)
        {
            ct.ThrowIfCancellationRequested();

            var client = new TcpClient { NoDelay = true };
            try
            {
                var remaining = deadline - DateTime.UtcNow;
                if (remaining <= TimeSpan.Zero) break;
                var attemptTimeout = remaining < ConnectAttemptTimeout ? remaining : ConnectAttemptTimeout;
                await client.ConnectAsync(deviceHost, port, ct).AsTask().WaitAsync(attemptTimeout, ct);
                return client;
            }
            catch (SocketException ex) when (IsTransientConnectError(ex.SocketErrorCode))
            {
                lastError = ex;
                client.Dispose();
                await Task.Delay(ConnectRetryDelay, ct);
            }
            catch (TimeoutException ex)
            {
                lastError = ex;
                client.Dispose();
                await Task.Delay(ConnectRetryDelay, ct);
            }
            catch
            {
                client.Dispose();
                throw;
            }
        }

        throw new IOException(
            $"Could not connect to device TCP endpoint {deviceHost}:{port} within {ConnectRetryWindow.TotalSeconds:0}s.",
            lastError);
    }

    private static bool IsTransientConnectError(SocketError error)
        => error is SocketError.NetworkUnreachable
            or SocketError.HostUnreachable
            or SocketError.ConnectionRefused
            or SocketError.TimedOut;
}
