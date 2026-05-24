namespace HyperLethal.Utilities;

public static class HyperLethalLog
{
    public static void Info(string component, string message) => Write("INFO", component, message);
    public static void Success(string component, string message) => Write("SUCCESS", component, message);
    public static void Warning(string component, string message) => Write("WARN", component, message);
    public static void Error(string component, string message) => Write("ERROR", component, message);
    
    private static void Write(string level, string component, string message)
    {
        Console.WriteLine($"[HyperLethal][{level}][{component}] {message}");
    }
}
