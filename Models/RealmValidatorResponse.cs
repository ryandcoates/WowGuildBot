namespace WowGuildBot.Models;

public class RealmValidationResult
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public string? RealmName { get; set; }
    public string? RealmSlug { get; set; }
}
