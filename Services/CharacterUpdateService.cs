public class CharacterUpdateService : BackgroundService
{
    private readonly ILogger<CharacterUpdateService> _logger;
    private readonly GuildCharacterStore _store;
    private readonly BlizzardApiService _api;

    public CharacterUpdateService(
        ILogger<CharacterUpdateService> logger,
        GuildCharacterStore store,
        BlizzardApiService api)
    {
        _logger = logger;
        _store = store;
        _api = api;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CharacterUpdateService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var characters = _store.GetAll().OrderBy(c => c.LastUpdated).ToList();
            _logger.LogInformation("Beginning update cycle for {Count} character(s).", characters.Count);

            foreach (var character in characters)
            {
                try
                {
                    _logger.LogInformation("Updating character: {Name} - {Realm} (LastUpdated: {LastUpdated})",
                        character.CharacterName, character.Realm, character.LastUpdated);

                    var slug = character.Realm.ToLower().Replace(" ", "-");
                    var profile = await _api.GetCharacterProfile(character.Region, slug, character.CharacterName.ToLower());

                    if (profile is not null &&
                        profile.Value.TryGetProperty("equipped_item_level", out var ilvlProp))
                    {
                        var newIlvl = ilvlProp.GetInt32();
                        character.ItemLevel = newIlvl;
                        character.LastUpdated = DateTime.UtcNow;

                        _logger.LogInformation("✅ Updated {Name} ({Realm}) to ilvl {ItemLevel}",
                            character.CharacterName, character.Realm, newIlvl);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Could not update ilvl for {Name} ({Realm}) — no ilvl returned",
                            character.CharacterName, character.Realm);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "❌ Failed to update character: {Name} - {Realm}",
                        character.CharacterName, character.Realm);
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); // throttle between characters
            }

            _logger.LogInformation("Update cycle complete. Sleeping until next run...");
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // idle between update cycles
        }
    }
}
