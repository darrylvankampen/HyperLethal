using HyperLethal.Services;

namespace HyperLethal.Utilities;

public static class LogService
{
    private static ConfigService? _configService;

    public static void Configure(ConfigService configService)
    {
        _configService = configService;
    }

    public static void Info(string component, string message) => Write("INFO", component, message, onlyWhenDebug: true);
    public static void Success(string component, string message) => Write("SUCCESS",component, message, onlyWhenDebug: true);
    public static void Warning(string component, string message) => Write("WARN", component, message, onlyWhenDebug: true);
    public static void Error(string component, string message) => Write("ERROR", component, message, onlyWhenDebug: true);

    public static void LoadedSuccessfully(string version)
    {
        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"HyperLethal {version} has loaded succesfully!");
        Console.ForegroundColor = previousColor;
    }

    private static void Write(string level, string component, string message, bool onlyWhenDebug)
    {
        var isDebug = _configService?.GetConfig().Development ?? false;
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
