using DSharpPlus.SlashCommands;

public class PingSlashCommand : ApplicationCommandModule
{
    [SlashCommand("ping", "Check if the bot is alive")]
    public async Task PingCommand(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync("🏓 Pong!");
    }
}