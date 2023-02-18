using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using MADS.Entities;
using MADS.Extensions;
using Microsoft.EntityFrameworkCore;

namespace MADS.Commands.Slash;

public class StarboardConfig : MadsBaseApplicationCommand
{
    public IDbContextFactory<MadsContext> ContextFactory { get; set; }

    private static Regex _emoteRegex = new Regex(@"^<(?<animated>a)?:(?<name>[a-zA-Z0-9_]+?):(?<id>\d+?)>$",
        RegexOptions.ECMAScript | RegexOptions.Compiled);


    [SlashCommand("Starboard", "Configure Starboard"),
     SlashRequirePermissions(Permissions.ManageGuild),
     SlashRequireGuild]
    public async Task StarboardConfigCommand
    (
        InteractionContext ctx,
        [Option("Channel", "Channel for starboard messages")]
        DiscordChannel channel,
        [Option("Emoji", "Emoji which is used as star (default: :star:)")]
        string emojiString = "⭐",
        [Option("Threshold", "Number of stars required for message (default: 3)")]
        long threshhold = 3
    )
    {
        await ctx.DeferAsync(true);

        var db = await ContextFactory.CreateDbContextAsync();

        var guildConfig = db.Configs.FirstOrDefault(x => x.DiscordGuildId == ctx.Guild.Id);

        DiscordEmoji emoji;
        if (!DiscordEmoji.TryFromUnicode(emojiString, out emoji))
        {
            var match = _emoteRegex.Match(emojiString);
            if (match.Success)
            {
                DiscordEmoji.TryFromGuildEmote(ctx.Client, ulong.Parse(match.Groups["id"].Value), out emoji);
            }
        }

        guildConfig.StarboardChannelId = channel.Id;
        guildConfig.StarboardThreshold = (int) threshhold;
        guildConfig.StarboardEmojiId = (ulong?) emoji.Id ?? 0;
        guildConfig.StarboardEmojiName = emoji.Name;
        guildConfig.StarboardActive = true;

        db.Configs.Update(guildConfig);
        db.SaveChanges();
        await db.DisposeAsync();

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Success"));
        return;
    }
}