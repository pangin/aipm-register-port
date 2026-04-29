using System.Diagnostics;
using System.Runtime.Versioning;

namespace AipmRegister.Wifi.MacOs;

/// Thin wrapper around the macOS `networksetup` CLI. Each public method
/// returns the trimmed stdout (or throws on non-zero exit) so callers can
/// stay in regular C# idioms instead of process plumbing.
[SupportedOSPlatform("macos")]
internal static class NetworksetupRunner
{
    public static async Task<string> RunAsync(string[] args, CancellationToken ct = default)
        => await RunInternalAsync("networksetup", args, ct);

    public static async Task<string> RunSystemProfilerAsync(CancellationToken ct = default)
        => await RunInternalAsync("system_profiler", new[] { "SPAirPortDataType", "-xml" }, ct);

    private static async Task<string> RunInternalAsync(string fileName, string[] args, CancellationToken ct)
    {
        var psi = new ProcessStartInfo(fileName)
        {
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
        };
        foreach (var a in args) psi.ArgumentList.Add(a);

        using var proc = Process.Start(psi)
            ?? throw new InvalidOperationException($"Failed to start '{fileName}'.");

        var stdoutTask = proc.StandardOutput.ReadToEndAsync(ct);
        var stderrTask = proc.StandardError.ReadToEndAsync(ct);
        await proc.WaitForExitAsync(ct);
        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        if (proc.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"{fileName} {string.Join(' ', args)} exited with {proc.ExitCode}: {stderr.Trim()}");
        }
        return stdout;
    }
}
