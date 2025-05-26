using System.Net.Http.Headers;
using System.Text.Json;

public class RealmCacheService : IHostedService
{
    private readonly ILogger<RealmCacheService> _logger;
    private readonly BlizzardApiService _api;
    private readonly HttpClient _http;
    private readonly string _filePath = Path.Combine(AppContext.BaseDirectory, "data", "realms.json");
    private readonly Dictionary<string, string> _nameToSlug = new(StringComparer.OrdinalIgnoreCase); // "Area 52" → "area-52"

    public RealmCacheService(
        ILogger<RealmCacheService> logger,
        BlizzardApiService api,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _api = api;
        _http = httpClientFactory.CreateClient();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

        try
        {
            _logger.LogInformation("Fetching realm list from Blizzard API...");
            var realms = await FetchRealmsFromApi();

            SaveToDisk(realms);
            PopulateCache(realms);
            _logger.LogInformation("Realm cache initialized with {Count} realms.", realms.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch realms. Attempting to load from cache...");
            var realms = LoadFromDisk();
            PopulateCache(realms);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task<List<RealmEntry>> FetchRealmsFromApi()
    {
        var token = await _api.GetAccessTokenAsync();
        var url = $"https://us.api.blizzard.com/data/wow/realm/index?namespace=dynamic-us&locale=en_US";
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var realms = doc.RootElement.GetProperty("realms");

        return realms.EnumerateArray()
            .Select(r => new RealmEntry
            {
                Id = r.GetProperty("id").GetInt32(),
                Name = r.GetProperty("name").GetString()!,
                Slug = r.GetProperty("slug").GetString()!
            })
            .ToList();
    }

    private void SaveToDisk(List<RealmEntry> realms)
    {
        var json = JsonSerializer.Serialize(realms, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }

    private List<RealmEntry> LoadFromDisk()
    {
        if (!File.Exists(_filePath))
            return new();

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<List<RealmEntry>>(json) ?? new();
    }

    private void PopulateCache(List<RealmEntry> realms)
    {
        _nameToSlug.Clear();
        foreach (var realm in realms)
        {
            _nameToSlug[realm.Name] = realm.Slug;
        }
    }

    // 🧠 Public helpers:

    public bool IsValidRealmName(string name) => _nameToSlug.ContainsKey(name);
    public string? GetSlugFor(string name) => _nameToSlug.TryGetValue(name, out var slug) ? slug : null;
    public IEnumerable<string> GetSuggestions(string startsWith) =>
        _nameToSlug.Keys.Where(n => n.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase));
}
