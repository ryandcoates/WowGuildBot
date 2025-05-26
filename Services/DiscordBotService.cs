using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class DiscordBotService : BackgroundService
{
    private readonly ILogger<DiscordBotService> _logger;
    private readonly IOptions<DiscordSettings> _discordOptions;
    private readonly TextCommandRouter _router;
    private readonly GuildCharacterStore _characterStore;
    private DiscordClient? _client;

    public DiscordBotService(
        ILogger<DiscordBotService> logger,
        IOptions<DiscordSettings> discordOptions,
        TextCommandRouter router,
        GuildCharacterStore characterStore)
    
    {
        _logger = logger;
        _discordOptions = discordOptions;
        _router = router;
        _characterStore = characterStore;

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new DiscordConfiguration
        {
            Token = _discordOptions.Value.Token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
        };

        _client = new DiscordClient(config);
        
        var slash = _client.UseSlashCommands();

        // For testing, register slash commands per guild (instant registration)
        ulong testGuildId = 1077028071980400670;
        slash.RegisterCommands<PingSlashCommand>(testGuildId);
        slash.RegisterCommands<RegisterSlashCommand>(testGuildId);
        slash.RegisterCommands<RosterSlashCommand>(testGuildId);
        slash.RegisterCommands<IlvlSlashCommand>(testGuildId);


        _client.Ready += async (s, e) =>
        {
            _logger.LogInformation("Bot is ready and connected.");
        };
        
        _client.GuildAvailable += async (s, e) =>
        {
            _logger.LogInformation("Connected to guild: {GuildName} ({GuildId})", e.Guild.Name, e.Guild.Id);
            _characterStore.LoadGuild(e.Guild.Id);
        };

        _client.MessageCreated += async (s, e) =>
        {
            if (e.Author.IsBot)
                return;

            await _router.RouteAsync(s, e);
        };

        await _client.ConnectAsync();
        _logger.LogInformation("Bot is connecting...");
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}