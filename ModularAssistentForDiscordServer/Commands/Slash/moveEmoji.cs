using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using MADS.Extensions;

namespace MADS.Commands.Slash;

public class MoveEmoji : MadsBaseApplicationCommand
{
    private const string EmojiRegex = @"<a?:(.+?):(\d+)>";

    [SlashCommand("MoveEmoji", "Move emoji to your guild"), SlashRequirePermissions(Permissions.ManageEmojis)]
    public async Task MoveEmojiAsync
        (InteractionContext ctx, [Option("Emoji", "Emoji which should be moved")] string pEmoji)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());

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
            catch (DiscordException) { }
        }

        if (!guilds.Any())
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("There are no guilds where you are able to add emojis"));
            return;
        }

        var options = guilds.Select(x => new DiscordSelectComponentOption(x.Name, x.Id.ToString())).ToList();

        //Create the select component and update our first response
        DiscordSelectComponent select = new("moveEmojiChooseGuild-" + ctx.User.Id,
            "Select guild", options, false, 0, options.Count());
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddComponents(select));

        //Get the initial response an wait for a component interaction
        var response = await ctx.GetOriginalResponseAsync();
        var selectResponse = await response.WaitForSelectAsync(ctx.Member, "moveEmojiChooseGuild-" + ctx.User.Id,
            TimeSpan.FromSeconds(60));

        //Notify the user when the interaction times out and abort
        if (selectResponse.TimedOut)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder
            {
                Content = "Timed out"
            });
            return;
        }

        //acknowledge interaction and edit first response to delete the select menu
        await selectResponse.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("Submitted").AsEphemeral());
        await ctx.EditResponseAsync(new DiscordWebhookBuilder
        {
            Content = "Submitted"
        });


        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("End"));
    }

    private static async Task CopyEmoji(DiscordClient client, string name, ulong id, bool animated, ulong targetGuild)
    {
        using HttpClient httpClient = new();
        var downloadedEmoji =
            await httpClient.GetStreamAsync($"https://cdn.discordapp.com/emojis/{id}.{(animated ? "gif" : "png")}");
        MemoryStream memory = new();
        await downloadedEmoji.CopyToAsync(memory);
        await downloadedEmoji.DisposeAsync();
        var targetGuildEntity = await client.GetGuildAsync(targetGuild);
        var _ = await targetGuildEntity.CreateEmojiAsync(name, memory);
    }
}