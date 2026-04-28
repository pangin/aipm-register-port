using AipmRegister.Core.Models;

namespace AipmRegister.Core.Devices;

/// Pushes the DeviceSettings payload to the IoT device's TCP server (port
/// 5000) while we are connected to its hotspot AP. Replaces the BeginRead /
/// BeginConnect / GetStream code in frmMain (around lines 1779, 1856).
public interface IDeviceTcpSender
{
    /// Connects, sends the settings, returns whatever the device replies
    /// (typically the model identifier).
    Task<string> SendSettingsAsync(
        string deviceHost,
        int port,
        DeviceSettings settings,
        CancellationToken ct = default);
}
