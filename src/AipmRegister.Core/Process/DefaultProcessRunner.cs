using SysProcess = System.Diagnostics.Process;
using SysProcessStartInfo = System.Diagnostics.ProcessStartInfo;

namespace AipmRegister.Core.Process;

/// Production <see cref="IProcessRunner"/>: spawns the requested
/// executable, captures its stdout/stderr asynchronously, and returns a
/// <see cref="ProcessResult"/>. Throws <see cref="InvalidOperationException"/>
/// if the process can't be started at all (e.g. the executable isn't on
/// PATH); a non-zero exit code is *not* an error here — callers decide
/// how to interpret it.
public sealed class DefaultProcessRunner : IProcessRunner
{
    public async Task<ProcessResult> RunAsync(
        string fileName,
        IReadOnlyList<string> args,
        CancellationToken ct = default)
    {
        var psi = new SysProcessStartInfo(fileName)
        {
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
        };
        foreach (var a in args) psi.ArgumentList.Add(a);

        using var proc = SysProcess.Start(psi)
            ?? throw new InvalidOperationException($"Failed to start '{fileName}'.");

        var stdoutTask = proc.StandardOutput.ReadToEndAsync(ct);
        var stderrTask = proc.StandardError.ReadToEndAsync(ct);
        await proc.WaitForExitAsync(ct);
        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        return new ProcessResult(proc.ExitCode, stdout, stderr);
    }
}
