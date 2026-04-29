namespace AipmRegister.Core.Process;

/// Captured result of a child process execution.
public sealed record ProcessResult(int ExitCode, string Stdout, string Stderr);

/// Abstraction over <c>System.Diagnostics.Process.Start</c>. Lets the
/// adapters and enumerators that shell out to <c>iw</c> /
/// <c>networksetup</c> / <c>system_profiler</c> stay testable on any host
/// — tests inject a stub that returns canned output, production wires
/// <see cref="DefaultProcessRunner"/>.
public interface IProcessRunner
{
    Task<ProcessResult> RunAsync(
        string fileName,
        IReadOnlyList<string> args,
        CancellationToken ct = default);
}
