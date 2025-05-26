using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public class BlizzardApiService
{
    private readonly HttpClient _http;
    private readonly ILogger<BlizzardApiService> _logger;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private string? _accessToken;
    private DateTime _expiresAt;

    public BlizzardApiService(HttpClient http, IConfiguration config, ILogger<BlizzardApiService> logger)
    {
        _http = http;
        _logger = logger;
        _clientId = config["Blizzard:ClientId"]!;
        _clientSecret = config["Blizzard:ClientSecret"]!;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        if (_accessToken != null && DateTime.UtcNow < _expiresAt)
            return _accessToken;

        var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
        var request = new HttpRequestMessage(HttpMethod.Post, "https://oauth.battle.net/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials"
            })
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        _accessToken = json.GetProperty("access_token").GetString();
        var expiresIn = json.GetProperty("expires_in").GetInt32();
        _expiresAt = DateTime.UtcNow.AddSeconds(expiresIn - 60);

        return _accessToken!;
    }

    public async Task<JsonElement?> GetCharacterProfile(string region, string realmSlug, string characterName)
    {
        var token = await GetAccessTokenAsync();
        var url = $"https://{region}.api.blizzard.com/profile/wow/character/{realmSlug}/{characterName.ToLower()}";
        var ns = $"profile-{region}";
        var uri = $"{url}?namespace={ns}&locale=en_US";

        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Failed to get character: {StatusCode}", response.StatusCode);
            return null;
        }

        var body = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(body).RootElement;
    }
}
