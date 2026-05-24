using HyperLethal.Constants;

namespace HyperLethal.Utilities;

public static class HyperLethalLog
{
    public static void Info(string component, string message) => Write("INFO", component, message, onlyWhenDebug: true);
    public static void Success(string component, string message) => Write("SUCCESS", component, message, onlyWhenDebug: true);
    public static void Warning(string component, string message) => Write("WARN", component, message, onlyWhenDebug: true);
    public static void Error(string component, string message) => Write("ERROR", component, message, onlyWhenDebug: true);

    public static void LoadedSuccessfully()
    {
        Console.WriteLine("HyperLethal has loaded succesfully");
    }

    private static void Write(string level, string component, string message)
    {
        Write(level, component, message, onlyWhenDebug: false);
    }

    private static void Write(string level, string component, string message, bool onlyWhenDebug)
    {
        var isDebug = HyperLethalDefaults.IsDev;
        if (onlyWhenDebug && !isDebug)
        {
            return;
        }

        Console.WriteLine($"[HyperLethal][{level}][{component}][isDebug:{isDebug}] {message}");
    }
}
