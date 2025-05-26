using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using WowGuildBot.Infrastructure;
using WowGuildBot.Services;
using Microsoft.Extensions.DependencyInjection;

public class IlvlSlashCommand : ApplicationCommandModule
{
    [SlashCommand("ilvl", "List registered characters sorted by item level")]
    public async Task Ilvl(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        var store = BotServiceLocator.Services.GetRequiredService<GuildCharacterStore>();
        var characters = store.GetForGuild(ctx.Guild.Id)
            .OrderByDescending(c => c.ItemLevel ?? 0)
            .ThenBy(c => c.CharacterName)
            .ToList();

        if (!characters.Any())
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent("📭 No characters registered in this server."));
            return;
        }

        var lines = characters.Select(c =>
        {
            var ilvlText = c.ItemLevel.HasValue ? $" (iLvl {c.ItemLevel})" : "";
            return $"- **{c.CharacterName}** - {c.Realm}{ilvlText}";
        });

        var message = string.Join("\n", lines.Take(25));
        if (characters.Count > 25)
            message += $"\n...and {characters.Count - 25} more";

        await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .WithContent($"🧱 **Item Level Roster** ({characters.Count}):\n{message}"));
    }
}