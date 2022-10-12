using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;

namespace MADS.Commands.Slash;

public class MoveEmoji : ApplicationCommandModule
{
    private const string EmojiRegex = @"<a?:(.+?):(\d+)>";
    
    public async Task MoveEmojiAsync(InteractionContext ctx, [Option("Emoji", "Emoji which should be moved")] string pEmoji)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());
        
        var matches = Regex.Matches(pEmoji, EmojiRegex);

        if (!matches.Any())
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("There are no emojis in your input"));
            return;
        }
        
        List<DiscordGuild> guilds = new();

        foreach (var guild in ctx.Client.Guilds.Values)
        {
            try
            {
                var member = await guild.GetMemberAsync(ctx.User.Id);
                if (member.Permissions.HasFlag(Permissions.ManageEmojis)) guilds.Add(guild);
            }
            catch (DiscordException){ }
        }
        
        if (!guilds.Any())
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("There are no guilds where you are able to add emojis"));
            return;
        }
        
        var options = guilds.Select(x => new DiscordSelectComponentOption(x.Name, x.Id.ToString()) ).ToList();

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("End"));
    }
}