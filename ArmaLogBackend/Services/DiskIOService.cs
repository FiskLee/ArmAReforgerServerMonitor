using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace ArmaLogBackend.Services;

/// <summary>
/// Provides disk I/O metrics. Linux uses iostat, Windows code is commented out.
/// </summary>
public static class DiskIOService
{
    public static (double readMBps, double writeMBps) GetDiskIO()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                var output = RunCommand("iostat", "-k 1 2");
                double readKB = 0, writeKB = 0;
                var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (Regex.IsMatch(line, @"^(sda|sd|nvme|vd)"))
                    {
                        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 4)
                        {
                            double.TryParse(parts[2], out double r);
                            double.TryParse(parts[3], out double w);
                            readKB += r;
                            writeKB += w;
                        }
                    }
                }
                return (readKB/1024.0, writeKB/1024.0);
            }
            catch
            {
                return (0,0);
            }
        }
        else
        {
            // Windows code => comment out or return (0,0) if no PerformanceCounter assembly
            return (0,0);
        }
    }

    private static string RunCommand(string cmd, string args)
    {
        var psi = new System.Diagnostics.ProcessStartInfo(cmd, args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        var p = System.Diagnostics.Process.Start(psi);
        p.WaitForExit();
        return p.StandardOutput.ReadToEnd();
    }
}
