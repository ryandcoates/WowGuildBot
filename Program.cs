using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using WowGuildBot.Infrastructure;
using WowGuildBot.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .AddUserSecrets<Program>();
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<DiscordSettings>(context.Configuration.GetSection("Discord"));
        services.AddHostedService<DiscordBotService>();
        services.AddHttpClient<BlizzardApiService>();
        services.AddSingleton<GuildCharacterStore>();
        services.AddHostedService<CharacterUpdateService>();

        services.AddSingleton<ITextCommand, PingCommand>();
        services.AddSingleton<ITextCommand, RegisterCommand>();
        services.AddSingleton<TextCommandRouter>();
        services.AddSingleton<RegisterSlashCommand>();

        services.AddHttpClient();
        services.AddSingleton<RealmCacheService>();
        services.AddHostedService(provider => provider.GetRequiredService<RealmCacheService>());

        services.AddSingleton<RealmValidatorService>();
        services.AddSingleton<CharacterRegistrationService>();
        
        services.AddSingleton<RosterSlashCommand>();
        services.AddSingleton<IlvlSlashCommand>();





    })
    .Build();
    
    BotServiceLocator.Services = host.Services;

    host.Run();