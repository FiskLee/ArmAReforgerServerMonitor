namespace ArmaLogBackend.Data;

/// <summary>
/// A single performance record with CPU usage, memory usage (MB), disk I/O, network I/O, etc.
/// </summary>
public class PerformanceRecord
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public double CpuUsage { get; set; }      // e.g. 40.1 => 40.1%
    public double MemoryUsage { get; set; }   // MB
    public double DiskReadMBs { get; set; }
    public double DiskWriteMBs { get; set; }
    public double NetworkInMBs { get; set; }
    public double NetworkOutMBs { get; set; }
}
