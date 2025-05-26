namespace WowGuildBot.Models;

public class GuildCharacter
{
    public ulong GuildId { get; set; }
    public ulong DiscordUserId { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public string Realm { get; set; } = string.Empty;
    public string Region { get; set; } = "us";
    public int? ItemLevel { get; set; }
    public int? WowGuildId { get; set; }
    public string? WowGuildName { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}