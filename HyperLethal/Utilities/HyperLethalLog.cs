using HyperLethal.Constants;

namespace HyperLethal.Utilities;

public static class HyperLethalLog
{
    public static void Info(string component, string message) => Write("INFO", component, message, onlyWhenDebug: true);
    public static void Success(string component, string message) => Write("SUCCESS",component, message, onlyWhenDebug: false);
    public static void Warning(string component, string message) => Write("WARN", component, message, onlyWhenDebug: true);
    public static void Error(string component, string message) => Write("ERROR", component, message, onlyWhenDebug: true);

    public static void LoadedSuccessfully(string version, int loadedItems, int loadedAssorts)
    {
        var line = $"HyperLethal {version} has loaded succesfully! Have fun ;) (Items: {loadedItems}, Assorts: {loadedAssorts})";
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(line);
        Console.ForegroundColor = previousColor;
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

        var line = $"[{level}][{component}] {message}";
        if (string.Equals(level, "SUCCESS", StringComparison.OrdinalIgnoreCase))
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(line);
            Console.ForegroundColor = previousColor;
            return;
        }

        Console.WriteLine(line);
    }
}
