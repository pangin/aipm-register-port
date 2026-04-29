using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Text;

namespace AipmRegister.Wifi.Linux;

/// Minimal client for the wpa_supplicant control socket interface
/// (https://w1.fi/wpa_supplicant/devel/ctrl_iface_page.html). Talks to the
/// per-interface unix-domain socket at /var/run/wpa_supplicant/&lt;iface&gt;.
///
/// Each command is a short ASCII string ("SCAN", "ADD_NETWORK",
/// "SET_NETWORK 0 ssid \"foo\"" ...) and the reply is one ASCII chunk.
/// We send/receive on a temp socket bound to /tmp/aipm-wpa-&lt;pid&gt;-&lt;n&gt;
/// so wpa_supplicant has somewhere to address its reply (it requires a
/// connected peer for `recvfrom` to work).
[SupportedOSPlatform("linux")]
internal sealed class WpaSupplicantClient : IDisposable
{
    private readonly string _interfaceName;
    private readonly string _serverPath;
    private readonly string _clientPath;
    private readonly Socket _socket;

    public WpaSupplicantClient(string interfaceName)
    {
        _interfaceName = interfaceName;
        _serverPath = $"/var/run/wpa_supplicant/{interfaceName}";
        _clientPath = $"/tmp/aipm-wpa-{Environment.ProcessId}-{Guid.NewGuid():N}";

        if (!File.Exists(_serverPath))
        {
            throw new FileNotFoundException(
                $"wpa_supplicant control socket not found at {_serverPath}. " +
                "Is wpa_supplicant running for this interface?", _serverPath);
        }

        _socket = new Socket(AddressFamily.Unix, SocketType.Dgram, ProtocolType.Unspecified);
        _socket.Bind(new UnixDomainSocketEndPoint(_clientPath));
        _socket.Connect(new UnixDomainSocketEndPoint(_serverPath));
        _socket.ReceiveTimeout = 3000;
    }

    public string InterfaceName => _interfaceName;

    public async Task<string> SendAsync(string command, CancellationToken ct = default)
    {
        var bytes = Encoding.ASCII.GetBytes(command);
        await _socket.SendAsync(bytes, SocketFlags.None, ct);

        var buffer = new byte[8192];
        var read = await _socket.ReceiveAsync(buffer, SocketFlags.None, ct);
        return Encoding.ASCII.GetString(buffer, 0, read).TrimEnd('\n');
    }

    public async Task<int> AddNetworkAsync(CancellationToken ct = default)
    {
        var raw = await SendAsync("ADD_NETWORK", ct);
        if (!int.TryParse(raw.Trim(), out var id))
        {
            throw new InvalidOperationException($"ADD_NETWORK returned non-integer: '{raw}'");
        }
        return id;
    }

    public async Task SetNetworkStringAsync(int id, string key, string value, CancellationToken ct = default)
    {
        var escaped = value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        var reply = await SendAsync($"SET_NETWORK {id} {key} \"{escaped}\"", ct);
        EnsureOk(reply, $"SET_NETWORK {id} {key} (string)");
    }

    public async Task SetNetworkRawAsync(int id, string key, string value, CancellationToken ct = default)
    {
        var reply = await SendAsync($"SET_NETWORK {id} {key} {value}", ct);
        EnsureOk(reply, $"SET_NETWORK {id} {key} (raw)");
    }

    public async Task SelectNetworkAsync(int id, CancellationToken ct = default)
    {
        var reply = await SendAsync($"SELECT_NETWORK {id}", ct);
        EnsureOk(reply, $"SELECT_NETWORK {id}");
    }

    public async Task EnableNetworkAsync(int id, CancellationToken ct = default)
    {
        var reply = await SendAsync($"ENABLE_NETWORK {id}", ct);
        EnsureOk(reply, $"ENABLE_NETWORK {id}");
    }

    public async Task RemoveNetworkAsync(int id, CancellationToken ct = default)
    {
        var reply = await SendAsync($"REMOVE_NETWORK {id}", ct);
        EnsureOk(reply, $"REMOVE_NETWORK {id}");
    }

    public async Task<string> ScanAsync(CancellationToken ct = default)
    {
        return await SendAsync("SCAN", ct);
    }

    public async Task<string> ScanResultsAsync(CancellationToken ct = default)
    {
        return await SendAsync("SCAN_RESULTS", ct);
    }

    public async Task<string> ListNetworksAsync(CancellationToken ct = default)
    {
        return await SendAsync("LIST_NETWORKS", ct);
    }

    public async Task<string> StatusAsync(CancellationToken ct = default)
    {
        return await SendAsync("STATUS", ct);
    }

    private static void EnsureOk(string reply, string command)
    {
        var trimmed = reply.Trim();
        if (trimmed != "OK")
        {
            throw new InvalidOperationException($"wpa_supplicant rejected {command}: '{trimmed}'");
        }
    }

    public void Dispose()
    {
        try { _socket.Dispose(); } catch { /* ignore */ }
        try { if (File.Exists(_clientPath)) File.Delete(_clientPath); } catch { /* ignore */ }
    }
}
