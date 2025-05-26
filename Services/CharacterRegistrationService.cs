using System.Text.Json;
using WowGuildBot.Models;

namespace WowGuildBot.Services;

public class CharacterRegistrationService
{
    private readonly RealmValidatorService _realmValidator;
    private readonly BlizzardApiService _blizzardApi;
    private readonly GuildCharacterStore _characterStore;
    
    public CharacterRegistrationService(
        RealmValidatorService realmValidator,
        BlizzardApiService blizzardApi,
        GuildCharacterStore characterStore)
    {
        _realmValidator = realmValidator;
        _blizzardApi = blizzardApi;
        _characterStore = characterStore;
    }
    
    public async Task<CharacterRegistrationResult> RegisterAsync(ulong guildId, ulong userId, string characterName, string realmName)
    {
        var realmCheck = _realmValidator.ValidateRealm(realmName);
        if (!realmCheck.IsValid)
        {
            return CharacterRegistrationResult.InvalidRealm(realmCheck.Message!);
        }

        var existing = _characterStore.FindByNameAndRealm(guildId, characterName, realmName);
        if (existing is not null)
        {
            return CharacterRegistrationResult.AlreadyRegistered(existing, existing.DiscordUserId == userId);
        }

        var profile = await _blizzardApi.GetCharacterProfile("us", realmCheck.RealmSlug!, characterName);
        if (profile is null)
        {
            return CharacterRegistrationResult.NotFound(characterName, realmName);
        }

        var character = CharacterFromProfile(profile.Value, characterName, realmName, guildId, userId);
        _characterStore.Add(character);

        return CharacterRegistrationResult.Success(character);
    }
    
    private GuildCharacter CharacterFromProfile(JsonElement profile, string name, string realm, ulong guildId, ulong userId)
    {
        int? ilvl = profile.TryGetProperty("equipped_item_level", out var ilvlProp) ? ilvlProp.GetInt32() : null;

        int? wowGuildId = null;
        string? wowGuildName = null;
        if (profile.TryGetProperty("guild", out var guild))
        {
            if (guild.TryGetProperty("id", out var idEl)) wowGuildId = idEl.GetInt32();
            if (guild.TryGetProperty("name", out var nameEl)) wowGuildName = nameEl.GetString();
        }

        return new GuildCharacter
        {
            GuildId = guildId,
            DiscordUserId = userId,
            CharacterName = name,
            Realm = realm,
            Region = "us",
            WowGuildId = wowGuildId,
            WowGuildName = wowGuildName,
            ItemLevel = ilvl,
            LastUpdated = DateTime.UtcNow
        };
    }
}