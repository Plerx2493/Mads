using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace MADS.Commands.Slash;

public class Ping : ApplicationCommandModule
{
    public MadsServiceProvider CommandService { get; set; }
    
    [SlashCommand("ping", "Get the bot's ping"), SlashCooldown(1, 60, SlashCooldownBucketType.User)]
    public async Task PingCommand(InteractionContext ctx)
    {
        var diff = DateTime.Now - CommandService.ModularDiscordBot.StartTime;
        var date = Humanizer.TimeSpanHumanizeExtensions.Humanize(diff); //$"{diff.Days} days {diff.Hours} hours {diff.Minutes} minutes";

        var discordEmbedBuilder = CommandUtility.GetDiscordEmbed();
        discordEmbedBuilder
            .WithTitle("Status")
            .WithTimestamp(DateTime.Now)
            .AddField("Uptime", date)
            .AddField("Websocket ping", $"{ctx.Client.Ping} ms");

        await ctx.CreateResponseAsync(discordEmbedBuilder, true);

        var response = await ctx.GetOriginalResponseAsync();
        await CommandService.ModularDiscordBot.Logging.LogCommandExecutionAsync(ctx, response);
        await Task.Delay(10_000);
        
        await ctx.DeleteResponseAsync();
    }
}