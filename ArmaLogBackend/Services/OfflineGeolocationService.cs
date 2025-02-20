using MaxMind.GeoIP2;

namespace ArmaLogBackend.Services;

/// <summary>
/// Offline geolocation using MaxMind DB. 
/// Provides GetCountryFromIP(ip).
/// </summary>
public static class OfflineGeolocationService
{
    private static DatabaseReader? _reader;

    public static void Initialize(string mmdbPath)
    {
        if (!File.Exists(mmdbPath))
        {
            throw new FileNotFoundException("MaxMind DB not found", mmdbPath);
        }
        _reader = new DatabaseReader(mmdbPath);
    }

    public static string GetCountryFromIP(string ip)
    {
        if (_reader == null)
            throw new InvalidOperationException("OfflineGeolocationService not initialized.");

        try
        {
            var resp = _reader.Country(ip);
            return resp.Country?.Name ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }
}
