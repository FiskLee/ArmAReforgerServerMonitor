using System.Text.RegularExpressions;
using ArmaLogBackend.Data;

namespace ArmaLogBackend.Services;

/// <summary>
/// Monitors console.log in real time, extracts FPS, CPU usage, memory usage, etc., 
/// stores them in PerformanceDbContext, calls AlarmService, etc.
/// </summary>
public static class LogMonitorService
{
    private static bool _running = false;
    private static Thread? _thread;
    private static string _logFilePath = "";

    public static void Start(string logFilePath, PerformanceDbContext perfDb)
    {
        _logFilePath = logFilePath;
        _running = true;
        _thread = new Thread(() => MonitorLogs(perfDb));
        _thread.IsBackground = true;
        _thread.Start();
    }

    private static void MonitorLogs(PerformanceDbContext perfDb)
    {
        try
        {
            if (!File.Exists(_logFilePath))
            {
                Console.WriteLine($"Log file not found: {_logFilePath}");
                return;
            }

            using var fs = new FileStream(_logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            fs.Seek(0, SeekOrigin.End);
            using var reader = new StreamReader(fs);

            while (_running)
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null) break;
                    ProcessLine(line, perfDb);
                }
                Thread.Sleep(1000);
            }
        }
        catch (Exception ex)
        {
            CrashLogger.LogCrash(ex, "LogMonitor");
        }
    }

    private static void ProcessLine(string line, PerformanceDbContext perfDb)
    {
        // Example: "FPS: 58.2, CPU: 40.1, MEM: 62, online=1"
        if (line.StartsWith("FPS:"))
        {
            double fpsVal = ExtractDouble(line, @"FPS:\s*(\d+(\.\d+)?)") ?? 60;
            double cpuVal = ExtractDouble(line, @"CPU:\s*(\d+(\.\d+)?)") ?? 30;
            double memVal = ExtractDouble(line, @"MEM:\s*(\d+(\.\d+)?)") ?? 50;
            bool serverOnline = !line.Contains("online=0");

            // DiskIO
            var (readMB, writeMB) = DiskIOService.GetDiskIO();
            double netIn = 0.12, netOut = 0.34; // example

            var rec = new PerformanceRecord
            {
                Timestamp = DateTime.UtcNow,
                CpuUsage = cpuVal,
                MemoryUsage = memVal * 1024.0, // store as MB => if we want to interpret line as "62" => 62 MB or 62000? your choice
                DiskReadMBs = readMB,
                DiskWriteMBs = writeMB,
                NetworkInMBs = netIn,
                NetworkOutMBs = netOut
            };
            perfDb.PerformanceRecords.Add(rec);
            perfDb.SaveChanges();

            AlarmService.CheckAlarms(fpsVal, cpuVal, serverOnline);
        }

        // "Player #342 name (IP) connected, BEGUID: x"
        if (line.Contains("Player #") && line.Contains("connected"))
        {
            var match = Regex.Match(line, @"Player #(\d+)\s+([^ ]+)\s+\(([^)]+)\)\s+connected");
            if (match.Success)
            {
                string playerName = match.Groups[2].Value;
                string ipPort = match.Groups[3].Value;
                string ip = ipPort.Split(':')[0];

                var beGuidMatch = Regex.Match(line, @"BEGUID:\s+(\S+)");
                string beGuid = beGuidMatch.Success ? beGuidMatch.Groups[1].Value : "0x0000";

                var p = PlayerYamlService.GetOrCreatePlayer(beGuid);
                p.Name = playerName;
                if (!p.KnownIPs.Contains(ip)) p.KnownIPs.Add(ip);
                if (!p.NameHistory.Contains(playerName)) p.NameHistory.Add(playerName);
                p.LastSeen = DateTime.UtcNow;
                p.TotalSessions += 1;

                p.Country = OfflineGeolocationService.GetCountryFromIP(ip);

                PlayerYamlService.SavePlayers();
            }
        }
    }

    private static double? ExtractDouble(string input, string pattern)
    {
        var match = Regex.Match(input, pattern);
        if (match.Success && double.TryParse(match.Groups[1].Value, out double val))
        {
            return val;
        }
        return null;
    }
}
