using AipmRegister.Core.Notification;

namespace AipmRegister.Cli;

internal sealed class ConsoleNotifier : IUserNotifier
{
    public void Info(string message)  => Write(ConsoleColor.Green,   "INFO",  message);
    public void Warn(string message)  => Write(ConsoleColor.Yellow,  "WARN",  message);

    public void Error(string message, Exception? cause = null)
    {
        Write(ConsoleColor.Red, "ERR ", message);
        if (cause is not null)
        {
            Write(ConsoleColor.DarkRed, "    ", cause.GetType().Name + ": " + cause.Message);
        }
    }

    public void Progress(string stage, string message)
    {
        Write(ConsoleColor.Cyan, $"[{stage}]", message);
    }

    private static void Write(ConsoleColor color, string tag, string message)
    {
        var prev = Console.ForegroundColor;
        try
        {
            Console.ForegroundColor = color;
            Console.Error.WriteLine($"{tag,-7} {message}");
        }
        finally
        {
            Console.ForegroundColor = prev;
        }
    }
}
