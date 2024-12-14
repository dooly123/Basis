using System;

/// <summary>
/// Basis Network Logger with Console Colors
/// </summary>
public static class BNL
{
    public static Action<string> LogOutput;
    public static Action<string> LogWarningOutput;
    public static Action<string> LogErrorOutput;

    private static string FormatMessage(string level, string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm");
        return $"[{timestamp}] [{level}] {message}";
    }

    public static void Log(string message)
    {
        string formattedMessage = FormatMessage("INFO", message);

        if (LogOutput != null)
        {
            LogOutput.Invoke(formattedMessage);
        }
        else
        {
            WriteWithColor(formattedMessage, ConsoleColor.White); // Info is white
        }
    }

    public static void LogWarning(string message)
    {
        string formattedMessage = FormatMessage("WARNING", message);

        if (LogWarningOutput != null)
        {
            LogWarningOutput.Invoke(formattedMessage);
        }
        else
        {
            WriteWithColor(formattedMessage, ConsoleColor.Yellow); // Warning is yellow
        }
    }

    public static void LogError(string message)
    {
        string formattedMessage = FormatMessage("ERROR", message);

        if (LogErrorOutput != null)
        {
            LogErrorOutput.Invoke(formattedMessage);
        }
        else
        {
            WriteWithColor(formattedMessage, ConsoleColor.Red); // Error is red
        }
    }

    private static void WriteWithColor(string message, ConsoleColor color)
    {
        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = originalColor;
    }
}