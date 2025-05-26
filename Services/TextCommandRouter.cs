using DSharpPlus;
using DSharpPlus.EventArgs;

public class TextCommandRouter
{
    private readonly List<ITextCommand> _commands;

    public TextCommandRouter(IEnumerable<ITextCommand> commands)
    {
        _commands = commands.ToList();
    }

    public async Task RouteAsync(DiscordClient client, MessageCreateEventArgs e)
    {
        foreach (var command in _commands)
        {
            if (command.CanHandle(e.Message.Content))
            {
                await command.HandleAsync(client, e);
                break;
            }
        }
    }
}