using WowGuildBot.Models;

public class CharacterRegistrationResult
{
    public bool IsSuccess { get; private init; }
    public bool IsAlreadyRegistered { get; private init; }
    public bool IsRegisteredBySelf { get; private init; }
    public string? Message { get; private init; }
    public GuildCharacter? Character { get; private init; }

    public static CharacterRegistrationResult Success(GuildCharacter character) =>
        new CharacterRegistrationResult
        {
            IsSuccess = true,
            Character = character
        };

    public static CharacterRegistrationResult NotFound(string name, string realm) =>
        new CharacterRegistrationResult
        {
            Message = $"❌ Character **{name} - {realm}** was not found in the Blizzard API."
        };

    public static CharacterRegistrationResult InvalidRealm(string message) =>
        new CharacterRegistrationResult
        {
            Message = message
        };

    public static CharacterRegistrationResult AlreadyRegistered(GuildCharacter character, bool bySelf) =>
        new CharacterRegistrationResult
        {
            IsAlreadyRegistered = true,
            IsRegisteredBySelf = bySelf,
            Character = character,
            Message = $"⚠️ Character **{character.CharacterName} - {character.Realm}** is already registered by {(bySelf ? "you" : $"<@{character.DiscordUserId}>")}."
        };
}