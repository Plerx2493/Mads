using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Humanizer;
using MADS.Extensions;

namespace MADS.Commands.Slash;

public class Ping : MadsBaseApplicationCommand
{
    [SlashCommand("ping", "Get the bot's ping"), SlashCooldown(1, 60, SlashCooldownBucketType.User)]
    public async Task PingCommand(InteractionContext ctx)
    {
        var diff = DateTime.Now - CommandService.StartTime;
        var date = diff.Humanize();

        var discordEmbedBuilder = CommandUtility.GetDiscordEmbed();
        discordEmbedBuilder
            .WithTitle("Status")
            .WithTimestamp(DateTime.Now)
            .AddField("Uptime", date)
            .AddField("Websocket ping", $"{ctx.Client.Ping} ms");

        await ctx.CreateResponseAsync(discordEmbedBuilder, true);

        await IntendedWait(10_000);
        
        await ctx.DeleteResponseAsync();
    }
}