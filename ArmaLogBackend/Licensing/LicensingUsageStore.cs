using System.Text.Json;

namespace ArmaLogBackend.Licensing;

/// <summary>
/// Stores usage for each license in licenseUsage.json, so usage persists across restarts.
/// </summary>
public class LicenseUsage
{
    public string Type { get; set; } = "SingleUse";
    public int MaxUsage { get; set; } = 1;
    public int UsedCount { get; set; } = 0;
    public DateTime Expires { get; set; } = DateTime.UtcNow.AddMonths(1);
}

public static class LicenseUsageStore
{
    private static readonly string UsagePath = "licenseUsage.json";
    private static Dictionary<string, LicenseUsage> _store = new();

    public static void Initialize()
    {
        if (File.Exists(UsagePath))
        {
            var json = File.ReadAllText(UsagePath);
            _store = JsonSerializer.Deserialize<Dictionary<string, LicenseUsage>>(json) ?? new();
        }
        else
        {
            _store = new();
        }
    }

    public static LicenseUsage GetOrCreateUsage(string licenseId, string type, int maxUsage, DateTime expires)
    {
        if (!_store.ContainsKey(licenseId))
        {
            _store[licenseId] = new LicenseUsage
            {
                Type = type,
                MaxUsage = maxUsage,
                UsedCount = 0,
                Expires = expires
            };
            Save();
        }
        return _store[licenseId];
    }

    public static void SaveUsage(string licenseId, LicenseUsage usage)
    {
        _store[licenseId] = usage;
        Save();
    }

    private static void Save()
    {
        var json = JsonSerializer.Serialize(_store, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(UsagePath, json);
    }
}
