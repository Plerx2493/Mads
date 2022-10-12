using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MADS.Entities;
using Microsoft.EntityFrameworkCore;

namespace MADS.Commands.Text.Base;

public class Ping : BaseCommandModule
{
    private MadsServiceProvider CommandService { get; set; }
    public IDbContextFactory<MadsContext> DbFactory { get; set; }

    [Command("ping"), Aliases("status"), Description("Get the ping of the websocket"),
     Cooldown(1, 30, CooldownBucketType.Channel)]
    public async Task PingCommand(CommandContext ctx)
    {
        var diff = DateTime.Now - CommandService.ModularDiscordBot.StartTime;
        var date = $"{diff.Days} days {diff.Hours} hours {diff.Minutes} minutes";

        var discordEmbedBuilder = CommandUtility.GetDiscordEmbed();
        discordEmbedBuilder
            .WithTitle("Status")
            .WithTimestamp(DateTime.Now)
            .AddField("Uptime", date)
            .AddField("Websocket ping", $"{ctx.Client.Ping} ms");

        var response = await ModularDiscordBot.AnswerWithDelete(ctx, discordEmbedBuilder.Build());
        await CommandService.ModularDiscordBot.Logging.LogCommandExecutionAsync(ctx, response);
    }
}