using System.Text.Json;
using WowGuildBot.Models;

public class GuildCharacterStore
{
    private readonly string _dataDir = Path.Combine(AppContext.BaseDirectory, "data");
    private readonly Dictionary<ulong, List<GuildCharacter>> _data = new();

    public GuildCharacterStore()
    {
        Directory.CreateDirectory(_dataDir);
        Console.WriteLine("Data directory: " + _dataDir);
    }

    private string GetFilePath(ulong guildId) =>
        Path.Combine(_dataDir, $"guild-{guildId}.json");

    public void Add(GuildCharacter character)
    {
        Console.WriteLine($"Adding character for {character.DiscordUserId} in guild {character.GuildId}");

        if (!_data.TryGetValue(character.GuildId, out var list))
        {
            list = new List<GuildCharacter>();
            _data[character.GuildId] = list;
        }

        list.Add(character);
        SaveToFile(character.GuildId);
    }

    public IEnumerable<GuildCharacter> GetAll()
        => _data.Values.SelectMany(x => x);

    public IEnumerable<GuildCharacter> GetForGuild(ulong guildId)
        => _data.TryGetValue(guildId, out var list) ? list : Enumerable.Empty<GuildCharacter>();

    public IEnumerable<GuildCharacter> GetForUser(ulong guildId, ulong userId)
        => GetForGuild(guildId).Where(c => c.DiscordUserId == userId);

    public void LoadGuild(ulong guildId)
    {
        var file = GetFilePath(guildId);
        if (File.Exists(file))
        {
            var json = File.ReadAllText(file);
            var list = JsonSerializer.Deserialize<List<GuildCharacter>>(json) ?? new();
            _data[guildId] = list;
        }
        else
        {
            _data[guildId] = new List<GuildCharacter>();
        }
    }
    
    public bool CharacterExists(GuildCharacter candidate)
    {
        return _data.TryGetValue(candidate.GuildId, out var list)
               && list.Any(c =>
                   c.DiscordUserId == candidate.DiscordUserId &&
                   string.Equals(c.CharacterName, candidate.CharacterName, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(c.Realm, candidate.Realm, StringComparison.OrdinalIgnoreCase));
    }
    
    public GuildCharacter? FindByNameAndRealm(ulong guildId, string characterName, string realm)
    {
        if (!_data.TryGetValue(guildId, out var list))
            return null;

        return list.FirstOrDefault(c =>
            string.Equals(c.CharacterName, characterName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(c.Realm, realm, StringComparison.OrdinalIgnoreCase));
    }

    private void SaveToFile(ulong guildId)
    {
        var path = GetFilePath(guildId);
        Console.WriteLine($"Saving to: {path}");

        if (_data.TryGetValue(guildId, out var list))
        {
            var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
    }
}