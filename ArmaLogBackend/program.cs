using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ArmaLogBackend.Data;
using ArmaLogBackend.Services;
// using ArmaLogBackend.Licensing; // If you want licensing

namespace ArmaLogBackend;

/// <summary>
/// The main entry point for the backend. 
/// Includes optional licensing with usage constraints & remote kill, offline geolocation, EF DB, log monitoring, minimal APIs.
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            // ============================
            // 1) Optional licensing usage store
            // ============================
            /*
            LicenseUsageStore.Initialize();

            Console.Write("Paste license JSON: ");
            var licenseJson = Console.ReadLine() ?? "";
            if (!LicensingService.ValidateAndConsumeLicense(licenseJson))
            {
                Console.WriteLine("License invalid or usage exceeded or remotely killed. Exiting...");
                return;
            }
            */

            // ============================
            // 2) Offline geolocation => load MaxMind DB
            // ============================
            OfflineGeolocationService.Initialize("GeoLite2-Country.mmdb");

            // ============================
            // 3) Initialize YAML-based players
            // ============================
            PlayerYamlService.Initialize("players.yaml");

            // ============================
            // 4) Create performance DB
            // ============================
            var perfDb = new PerformanceDbContext("performance.sqlite");

            // ============================
            // 5) Start log monitoring => parse console.log lines
            // ============================
            var logFilePath = "/var/log/arma-reforger/console.log"; // example path
            LogMonitorService.Start(logFilePath, perfDb);

            // ============================
            // 6) Build minimal API
            // ============================
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            // Global crash logs => no inline pipeline usage
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var ex = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                    if (ex != null)
                    {
                        CrashLogger.LogCrash(ex, "Global");
                    }
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Internal Server Error => see crash logs.\n");
                });
            });

            // Root route => avoid 404 at "/"
            app.MapGet("/", () => "ArmaLogBackend is running.");

            // Return last 50 performance records
            app.MapGet("/api/performance", (PerformanceDbContext db, HttpContext ctx) =>
            {
                var timeRange = ctx.Request.Query["timeRange"].ToString() ?? "Last 10 min";
                var recs = db.PerformanceRecords
                    .OrderByDescending(r => r.Timestamp)
                    .Take(50)
                    .ToList();
                return recs;
            });

            // Return daily usage => group by date
            app.MapGet("/api/usage/daily", (PerformanceDbContext db) =>
            {
                var grouped = db.PerformanceRecords
                    .AsEnumerable()
                    .GroupBy(r => r.Timestamp.Date)
                    .Select(g => new {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        AvgCpu = g.Average(x => x.CpuUsage),
                        AvgMem = g.Average(x => x.MemoryUsage),
                        Count = g.Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToList();
                return grouped;
            });

            // Return players from YAML
            app.MapGet("/api/players/leaderboard", () =>
            {
                var all = PlayerYamlService.GetAllPlayers().Values;
                var top = all.OrderByDescending(p => p.TotalPlaytimeHours).Take(50).ToList();
                return top;
            });

            // Raw data => last 20 lines of console.log
            app.MapGet("/api/rawdata", () =>
            {
                if (!File.Exists(logFilePath))
                    return new List<string> { "No console.log found" };

                var lines = File.ReadAllLines(logFilePath);
                if (lines.Length <= 20) return lines;
                return lines.Skip(lines.Length - 20).ToArray();
            });

            // Uptime => since the program started
            app.MapGet("/api/uptime", () =>
            {
                var uptime = DateTime.UtcNow - SummariesService.StartTime;
                return new { Uptime = uptime.ToString(@"dd\.hh\:mm\:ss") };
            });

            // Return last 100 lines of "backend" logs
            app.MapGet("/api/logs/backend", () =>
            {
                if (!File.Exists(logFilePath))
                    return new List<string> { "No console.log found" };

                var lines = File.ReadAllLines(logFilePath);
                if (lines.Length <= 100) return lines;
                return lines.Skip(lines.Length - 100).ToArray();
            });

            Console.WriteLine("Backend on http://0.0.0.0:5000");
            app.Urls.Add("http://0.0.0.0:5000");
            app.Run();
        }
        catch (Exception ex)
        {
            CrashLogger.LogCrash(ex, "Main");
        }
    }
}
