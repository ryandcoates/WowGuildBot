using DSharpPlus;
using DSharpPlus.EventArgs;
using WowGuildBot.Models;

public class RegisterCommand : ITextCommand
{
    private readonly BlizzardApiService _api;
    private readonly GuildCharacterStore _store;

    public RegisterCommand(BlizzardApiService api, GuildCharacterStore store)
    {
        _api = api;
        _store = store;
    }

    public bool CanHandle(string input) => input.StartsWith("!register", StringComparison.OrdinalIgnoreCase);

    public async Task HandleAsync(DiscordClient client, MessageCreateEventArgs e)
    {
        var parts = e.Message.Content.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
        {
            await e.Message.RespondAsync("Usage: `!register <CharacterName> <Realm>`");
            return;
        }

        var character = parts[1];
        var realm = parts[2];
        var realmSlug = realm.ToLower().Replace(" ", "-");

        var profile = await _api.GetCharacterProfile("us", realmSlug, character);
        if (profile == null)
        {
            await e.Message.RespondAsync("❌ Could not find that character.");
            return;
        }

        var race = profile.Value.GetProperty("race").GetProperty("name").GetString();
        var className = profile.Value.GetProperty("character_class").GetProperty("name").GetString();
        var level = profile.Value.GetProperty("level").GetInt32();
        int? wowGuildId = null;
        string? wowGuildName = null;

        if (profile.Value.TryGetProperty("guild", out var guildElement))
        {
            if (guildElement.TryGetProperty("id", out var idElement))
                wowGuildId = idElement.GetInt32();

            if (guildElement.TryGetProperty("name", out var nameElement))
                wowGuildName = nameElement.GetString();
        }
        
        int? ilvl = null;
        if (profile.Value.TryGetProperty("equipped_item_level", out var ilvlProp))
            ilvl = ilvlProp.GetInt32();

        var characterData = new GuildCharacter
        {
            GuildId = e.Guild.Id,
            DiscordUserId = e.Author.Id,
            CharacterName = character,
            Realm = realm,
            WowGuildId = wowGuildId,
            WowGuildName = wowGuildName,
            ItemLevel = ilvl,
            LastUpdated = DateTime.UtcNow
        };
        
        if (_store.CharacterExists(characterData))
        {
            await e.Message.RespondAsync($"⚠️ You’ve already registered **{character}** on **{realm}**.");
            return;
        }
        _store.Add(characterData);

        await e.Message.RespondAsync($"✅ Registered **{character}** on **{realm}** — Level {level} {race} {className} ({ilvl})");

        var all = _store.GetAll();
        Console.WriteLine("--- Registered Characters ---");
        foreach (var c in all)
            Console.WriteLine($"{c.GuildId} | {c.DiscordUserId} => {c.CharacterName} ({c.ItemLevel}) - {c.Realm}");
    }
}
