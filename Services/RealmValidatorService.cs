using WowGuildBot.Models;

public class RealmValidatorService
{
    private readonly RealmCacheService _cache;

    public RealmValidatorService(RealmCacheService cache)
    {
        _cache = cache;
    }

    public bool IsValidRealmName(string name) => _cache.IsValidRealmName(name);

    public string? GetSlugForRealm(string name) => _cache.GetSlugFor(name);

    public RealmValidationResult ValidateRealm(string name)
    {
        if (!_cache.IsValidRealmName(name))
        {
            return new RealmValidationResult
            {
                IsValid = false,
                Message = $"❌ Realm \"{name}\" is not recognized. Please check the spelling or try another realm."
            };
        }

        return new RealmValidationResult
        {
            IsValid = true,
            RealmName = name,
            RealmSlug = _cache.GetSlugFor(name)!
        };
    }
    
    
}