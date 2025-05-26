using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using WowGuildBot.Infrastructure;
using WowGuildBot.Services;
using Microsoft.Extensions.DependencyInjection;

public class RosterSlashCommand : ApplicationCommandModule
{
    [SlashCommand("roster", "List registered characters alphabetically")]
    public async Task Roster(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        var store = BotServiceLocator.Services.GetRequiredService<GuildCharacterStore>();
        var characters = store.GetForGuild(ctx.Guild.Id)
            .OrderBy(c => c.CharacterName)
            .ThenBy(c => c.Realm)
            .ToList();

        if (!characters.Any())
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent("📭 No characters registered in this server."));
            return;
        }

        var lines = characters.Select(c => $"- **{c.CharacterName}** - {c.Realm}");
        var message = string.Join("\n", lines.Take(25));
        if (characters.Count > 25)
            message += $"\n...and {characters.Count - 25} more";

        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent($"📋 **Roster** ({characters.Count}):\n{message}"));
    }
}