using System.Net.Sockets;

namespace AipmRegister.Core.Network;

public sealed class TcpInternetReachabilityProbe : IInternetReachabilityProbe
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan ConnectAttemptTimeout = TimeSpan.FromSeconds(3);

    public async Task WaitUntilReachableAsync(
        string host,
        int port,
        TimeSpan timeout,
        CancellationToken ct = default)
    {
        var deadline = DateTime.UtcNow + timeout;
        Exception? lastError = null;

        while (DateTime.UtcNow < deadline)
        {
            ct.ThrowIfCancellationRequested();

            using var client = new TcpClient { NoDelay = true };
            try
            {
                var remaining = deadline - DateTime.UtcNow;
                if (remaining <= TimeSpan.Zero) break;
                var attemptTimeout = remaining < ConnectAttemptTimeout ? remaining : ConnectAttemptTimeout;
                await client.ConnectAsync(host, port, ct).AsTask().WaitAsync(attemptTimeout, ct);
                return;
            }
            catch (SocketException ex) when (IsTransientSocketError(ex.SocketErrorCode))
            {
                lastError = ex;
            }
            catch (TimeoutException ex)
            {
                lastError = ex;
            }
            catch (IOException ex)
            {
                lastError = ex;
            }

            await Task.Delay(RetryDelay, ct);
        }

        throw new IOException(
            $"Internet connection to {host}:{port} did not become reachable within {timeout.TotalSeconds:0}s.",
            lastError);
    }

    private static bool IsTransientSocketError(SocketError error)
        => error is SocketError.NetworkUnreachable
            or SocketError.HostUnreachable
            or SocketError.HostNotFound
            or SocketError.TryAgain
            or SocketError.TimedOut
            or SocketError.ConnectionRefused;
}
