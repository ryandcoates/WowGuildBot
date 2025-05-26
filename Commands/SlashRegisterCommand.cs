using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using WowGuildBot.Infrastructure;
using WowGuildBot.Models;
using WowGuildBot.Services;

public class RegisterSlashCommand : ApplicationCommandModule
{
    
    
    [SlashCommand("register", "Link your WoW character to your Discord account")]
    public async Task Register(
        InteractionContext ctx,
        [Option("character", "Your WoW character name")] string character,
        [Option("realm", "Your realm (e.g. Area 52)")] string realm)
    {
        var registrationService = BotServiceLocator.Services.GetRequiredService<CharacterRegistrationService>();

        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        try
        {
            var result = await registrationService.RegisterAsync(
                guildId: ctx.Guild.Id,
                userId: ctx.User.Id,
                characterName: character,
                realmName: realm);

            if (!result.IsSuccess)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent(result.Message ?? "❌ Registration failed."));
                return;
            }

            var c = result.Character!;
            var successMessage = $"✅ Registered **{c.CharacterName}** on **{c.Realm}**";

            if (c.ItemLevel.HasValue)
                successMessage += $" — iLvl {c.ItemLevel}";

            if (!string.IsNullOrWhiteSpace(c.WowGuildName))
                successMessage += $" — Guild: {c.WowGuildName}";

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(successMessage));
        }
        catch (Exception ex)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"❌ An unexpected error occurred: {ex.Message}"));
        }
    }
}

    

