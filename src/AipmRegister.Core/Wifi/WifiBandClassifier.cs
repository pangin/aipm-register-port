namespace AipmRegister.Core.Wifi;

/// Maps wireless radio inputs to a canonical band label
/// (<c>"2.4G"</c> / <c>"5G"</c> / <c>"6G"</c> / <c>"?"</c>).
///
/// Two entry points because each platform reports the radio differently:
///   - Windows + Linux supply a frequency in MHz (2412, 5180, 6075, …),
///     so call <see cref="FromFrequencyMhz"/>.
///   - macOS' <c>system_profiler</c> reports a channel string with an
///     optional band suffix ("36 (5 GHz)", "1 (2.4 GHz, 6 GHz, 20 MHz)"),
///     so call <see cref="FromChannel"/>.
///
/// Centralising both keeps the band convention identical across the three
/// adapters and makes a future fourth platform a one-call change.
public static class WifiBandClassifier
{
    public const string Unknown = "?";
    public const string Band24  = "2.4G";
    public const string Band5   = "5G";
    public const string Band6   = "6G";

    public static string FromFrequencyMhz(int freqMhz) => freqMhz switch
    {
        >= 2400 and < 2500 => Band24,
        >= 5000 and < 6000 => Band5,
        >= 6000            => Band6,
        _                  => Unknown,
    };

    /// Channel 1-14 → 2.4G, 32-196 → 5G, 1-233 → 6G (overlap with 2.4G,
    /// but system_profiler tags 6 GHz channels with " (6 GHz)" suffix in
    /// newer macOS versions — we sniff for that suffix first).
    public static string FromChannel(string? channel)
    {
        if (string.IsNullOrEmpty(channel)) return Unknown;
        var compact = channel.Replace(" ", string.Empty, StringComparison.Ordinal);
        if (compact.Contains("6GHz",   StringComparison.OrdinalIgnoreCase)) return Band6;
        if (compact.Contains("5GHz",   StringComparison.OrdinalIgnoreCase)) return Band5;
        if (compact.Contains("2.4GHz", StringComparison.OrdinalIgnoreCase)
            || compact.Contains("2GHz", StringComparison.OrdinalIgnoreCase))
        {
            return Band24;
        }

        // Numeric channel only — fall back to canonical bands.
        var token = channel.Split(',', '(', ' ')[0].Trim();
        if (int.TryParse(token, out var n))
        {
            return n switch
            {
                >= 1 and <= 14   => Band24,
                >= 32 and <= 196 => Band5,
                >= 1 and <= 233  => Band6,
                _                => Unknown,
            };
        }
        return Unknown;
    }
}
