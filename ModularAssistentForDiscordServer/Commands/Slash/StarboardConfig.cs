using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using MADS.Entities;
using MADS.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace MADS.Commands.Slash;


public class StarboardConfig : MadsBaseApplicationCommand
{
    public IDbContextFactory<MadsContext> ContextFactory { get; set; }

    [SlashCommand("Starboard", "Configure Starboard"),
     SlashRequirePermissions(Permissions.ManageGuild),
     SlashRequireGuild]
    public async Task RoleSelectionCreation
    (
        InteractionContext ctx,
        [Option("Channel", "Channel for starboard messages")]
        DiscordChannel channel,
        [Option("Emoji", "Emoji which is used as star (default: :star:)")]
        string emoji = ":star:",
        [Option("Threshold", "Number of stars required for message (default: 3)")]
        long threshhold = 3
    )
    {
        await ctx.DeferAsync(true);
        
        var db = await ContextFactory.CreateDbContextAsync();
        
        var guildConfig = db.Guilds.FirstOrDefault(x => x.DiscordId == ctx.Guild.Id);
        
        guildConfig.Settings.StarboardChannelId = channel.Id;
        guildConfig.Settings.StarboardThreshold = (int)threshhold;
        guildConfig.Settings.StarboardEmojiId = emoji;
        
        db.Upsert(guildConfig);
        
        await db.DisposeAsync();
        
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Success"));
        return;
    }
}