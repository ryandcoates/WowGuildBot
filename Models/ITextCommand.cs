using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

public interface ITextCommand
{
    bool CanHandle(string input);
    Task HandleAsync(DiscordClient client, MessageCreateEventArgs e);
}