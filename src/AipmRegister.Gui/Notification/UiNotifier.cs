using System.Collections.ObjectModel;
using AipmRegister.Core.Notification;
using Avalonia.Threading;

namespace AipmRegister.Gui.Notification;

public sealed class LogEntry
{
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public string   Level     { get; init; } = "info";
    public string   Stage     { get; init; } = "";
    public string   Message   { get; init; } = "";

    public string Display => $"{Timestamp:HH:mm:ss} [{Level.ToUpperInvariant(),-5}] {(string.IsNullOrEmpty(Stage) ? "" : $"({Stage}) ")}{Message}";
}

/// IUserNotifier implementation that pushes log entries to an observable
/// collection bound to the GUI. Always marshals to the UI thread so the
/// orchestrator can call us freely from background tasks.
public sealed class UiNotifier : IUserNotifier
{
    public ObservableCollection<LogEntry> Entries { get; } = new();

    public void Info(string message)  => Append("info",  string.Empty, message);
    public void Warn(string message)  => Append("warn",  string.Empty, message);

    public void Error(string message, Exception? cause = null)
    {
        Append("error", string.Empty, message);
        if (cause is not null)
        {
            Append("error", string.Empty, $"  {cause.GetType().Name}: {cause.Message}");
        }
    }

    public void Progress(string stage, string message) => Append("info", stage, message);

    private void Append(string level, string stage, string message)
    {
        var entry = new LogEntry { Level = level, Stage = stage, Message = message };
        if (Dispatcher.UIThread.CheckAccess())
        {
            Entries.Add(entry);
        }
        else
        {
            Dispatcher.UIThread.Post(() => Entries.Add(entry));
        }
    }
}
