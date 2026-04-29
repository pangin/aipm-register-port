using AipmRegister.Core.Wifi;

namespace AipmRegister.Wifi.MacOs;

/// Pure parser for `networksetup -listallhardwareports`, separated from
/// the I/O so it's unit-testable without a macOS host.
///
/// Input shape (each block separated by a blank line):
///
///   Hardware Port: Wi-Fi
///   Device: en0
///   Ethernet Address: 11:22:33:44:55:66
///
///   Hardware Port: Ethernet
///   Device: en5
///   Ethernet Address: aa:bb:cc:dd:ee:ff
///
/// We only return Wi-Fi entries — any block whose Hardware Port label
/// contains "Wi-Fi" (case-insensitive). The returned
/// <see cref="WifiInterface.Id"/> is the BSD device name (en0, en7, …);
/// <see cref="WifiInterface.DisplayName"/> is "{port} ({device})" so
/// multiple Wi-Fi entries on the same machine remain
/// human-distinguishable.
public static class HardwarePortsParser
{
    private const string HardwarePortPrefix = "Hardware Port:";
    private const string DevicePrefix       = "Device:";
    private const string WifiToken          = "Wi-Fi";

    public static IReadOnlyList<WifiInterface> Parse(string listAllHardwarePortsOutput)
    {
        if (string.IsNullOrWhiteSpace(listAllHardwarePortsOutput)) return Array.Empty<WifiInterface>();

        var lines = listAllHardwarePortsOutput.Split('\n');
        var result = new List<WifiInterface>();

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (!line.StartsWith(HardwarePortPrefix, StringComparison.Ordinal)) continue;

            var portLabel = line.Substring(HardwarePortPrefix.Length).Trim();
            if (portLabel.IndexOf(WifiToken, StringComparison.OrdinalIgnoreCase) < 0) continue;

            // The Device line follows — usually immediately, but tolerate
            // blank/extra lines defensively.
            for (var j = i + 1; j < Math.Min(i + 4, lines.Length); j++)
            {
                var candidate = lines[j].Trim();
                if (!candidate.StartsWith(DevicePrefix, StringComparison.Ordinal)) continue;

                var device = candidate.Substring(DevicePrefix.Length).Trim();
                if (string.IsNullOrEmpty(device)) break;

                result.Add(new WifiInterface(
                    Id:          device,
                    DisplayName: $"{portLabel} ({device})"));
                break;
            }
        }
        return result;
    }
}
