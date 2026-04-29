using System.Runtime.Versioning;
using AipmRegister.Core.Process;

namespace AipmRegister.Wifi.MacOs;

/// Thin wrapper around the macOS `networksetup` and `system_profiler`
/// CLIs. Runs through an injected <see cref="IProcessRunner"/> so callers
/// can swap in a stub for tests; the runner-side error handling
/// (non-zero exit code) is centralised here so individual adapters stay
/// in regular C# idioms instead of process plumbing.
[SupportedOSPlatform("macos")]
internal static class NetworksetupRunner
{
    public static Task<string> RunAsync(IProcessRunner runner, string[] args, CancellationToken ct = default)
        => RunInternalAsync(runner, "networksetup", args, ct);

    public static Task<string> RunSystemProfilerAsync(IProcessRunner runner, CancellationToken ct = default)
        => RunInternalAsync(runner, "system_profiler", new[] { "SPAirPortDataType", "-xml" }, ct);

    private static async Task<string> RunInternalAsync(
        IProcessRunner runner, string fileName, string[] args, CancellationToken ct)
    {
        var result = await runner.RunAsync(fileName, args, ct);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"{fileName} {string.Join(' ', args)} exited with {result.ExitCode}: {result.Stderr.Trim()}");
        }
        return result.Stdout;
    }
}
