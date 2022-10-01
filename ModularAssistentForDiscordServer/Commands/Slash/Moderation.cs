using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace MADS.Commands.Slash;

[SlashCommandGroup("Moderation", "Commands to moderate a guild"), GuildOnly]
internal class ModerationSlashCommands : ApplicationCommandModule
{
    [SlashCommand("test", "test smth")]
    public async Task TestCommand(InteractionContext ctx)
    {
        var tmp = await ctx.Client.GetGlobalApplicationCommandsAsync();
        foreach (var cmd in tmp)
        {
            await ctx.Client.DeleteGlobalApplicationCommandAsync(cmd.Id);
        }

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