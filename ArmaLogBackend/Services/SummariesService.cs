namespace ArmaLogBackend.Services;

/// <summary>
/// Tracks the time the backend started => used for uptime
/// </summary>
public static class SummariesService
{
    public static readonly DateTime StartTime = DateTime.UtcNow;
}
