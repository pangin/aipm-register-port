namespace AipmRegister.Core.Wifi;

/// Optional extension for adapters that can report the gateway assigned to
/// the selected wireless interface after joining an AP. The original Windows
/// tool used the Wi-Fi gateway as the IoT device TCP host instead of assuming
/// a fixed address.
public interface IWifiGatewayProvider
{
    Task<string?> GetGatewayAsync(TimeSpan timeout, CancellationToken ct = default);
}
