using Microsoft.EntityFrameworkCore;

namespace ArmaLogBackend.Data;

/// <summary>
/// EF Core DbContext for performance records.
/// </summary>
public class PerformanceDbContext : DbContext
{
    private readonly string _dbPath;

    public DbSet<PerformanceRecord> PerformanceRecords => Set<PerformanceRecord>();

    public PerformanceDbContext(string dbPath)
    {
        _dbPath = dbPath;
        Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={_dbPath}");
    }
}
