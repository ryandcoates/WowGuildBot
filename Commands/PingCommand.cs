using DSharpPlus;
using DSharpPlus.EventArgs;

public class PingCommand : ITextCommand
{
    public bool CanHandle(string input) => input.StartsWith("!ping", StringComparison.OrdinalIgnoreCase);

    public async Task HandleAsync(DiscordClient client, MessageCreateEventArgs e)
    {
        await e.Message.RespondAsync("Pong!");
    }
}