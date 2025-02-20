namespace ArmaLogBackend.Services;

/// <summary>
/// Writes crash logs to crashlogs/ folder if an unhandled exception occurs.
/// </summary>
public static class CrashLogger
{
    public static void LogCrash(Exception ex, string sessionId)
    {
        var dir = "crashlogs";
        Directory.CreateDirectory(dir);
        var file = Path.Combine(dir, $"{DateTime.UtcNow:yyyy-MM-dd}_{sessionId}_crash.log");
        File.AppendAllText(file, $"[{DateTime.UtcNow:HH:mm:ss}] {ex}\n");
    }
}
