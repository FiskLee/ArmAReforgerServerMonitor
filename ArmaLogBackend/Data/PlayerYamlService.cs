using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ArmaLogBackend.Data;

/// <summary>
/// Data model for a player read from players.yaml
/// </summary>
public class PlayerData
{
    public string BEGUID { get; set; } = "";
    public string Name { get; set; } = "";
    public List<string> KnownIPs { get; set; } = new();
    public List<string> NameHistory { get; set; } = new();
    public DateTime FirstSeen { get; set; } = DateTime.UtcNow;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public int TotalSessions { get; set; }
    public double TotalPlaytimeHours { get; set; }
    public double LongestSessionHours { get; set; }
    public List<string> Notes { get; set; } = new();
    public string Country { get; set; } = "Unknown";
}

/// <summary>
/// Loads players.yaml, provides methods to read/write players, etc.
/// </summary>
public static class PlayerYamlService
{
    private static string _yamlPath = "players.yaml";
    private static Dictionary<string, PlayerData> _players = new();

    public static void Initialize(string yamlPath)
    {
        _yamlPath = yamlPath;
        if (File.Exists(_yamlPath))
        {
            var content = File.ReadAllText(_yamlPath);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            _players = deserializer.Deserialize<Dictionary<string, PlayerData>>(content) ?? new();
        }
        else
        {
            _players = new();
            SavePlayers();
        }
    }

    public static void SavePlayers()
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var yaml = serializer.Serialize(_players);
        File.WriteAllText(_yamlPath, yaml);
    }

    public static Dictionary<string, PlayerData> GetAllPlayers() => _players;

    public static PlayerData GetOrCreatePlayer(string beGuid)
    {
        if (!_players.ContainsKey(beGuid))
        {
            _players[beGuid] = new PlayerData { BEGUID = beGuid, FirstSeen = DateTime.UtcNow };
        }
        return _players[beGuid];
    }
}
