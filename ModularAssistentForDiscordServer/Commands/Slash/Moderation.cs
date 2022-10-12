using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MADS.Entities;
using Microsoft.EntityFrameworkCore;

namespace MADS.Commands.Slash;

[SlashCommandGroup("Moderation", "Commands to moderate a guild"), GuildOnly]
public class ModerationSlashCommands : ApplicationCommandModule
{
    public IDbContextFactory<MadsContext> DbFactory { get; set; }
    
    [SlashCommand("test", "test smth")]
    public async Task TestCommand(InteractionContext ctx)
    {
        var dbcontext = DbFactory.CreateDbContext();
        var tmp = dbcontext.Guilds.First(x => x.Id == 0);

        var discordEmbed = new DiscordEmbedBuilder
        {
            Title = "Test",
            Description = "Test executed",
            Color = DiscordColor.Blue,
            Timestamp = DateTime.Now
        };

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AddEmbed(discordEmbed));
    }

    /*
    [SlashCommand("freeze", "Freezes a conversation in a moderation channel")]
    public async Task FreezeCommand(InteractionContext ctx, [Option("number_of_messages", "Number of messages which should be freezed")] long messages, [Option("channel", "Channel where the messages should be freezed")] DiscordChannel discordChannel = null)
    {
    }
    */
}