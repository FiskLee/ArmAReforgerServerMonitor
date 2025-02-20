namespace ArmaLogBackend.Services;

/// <summary>
/// Example alarm service => triggers if server offline or fps < 10, etc.
/// </summary>
public static class AlarmService
{
    public static void CheckAlarms(double fps, double cpuUsage, bool serverOnline)
    {
        if (!serverOnline || fps < 10.0)
        {
            Console.WriteLine("[ALARM] Server offline or FPS < 10 => send alert!");
            // Real usage => email, Slack, etc.
        }
    }
}
