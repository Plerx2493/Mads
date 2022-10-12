using System.Diagnostics;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Humanizer.Localisation;
using MADS.Entities;
using Microsoft.EntityFrameworkCore;


namespace MADS.Commands.Text.Base;

public class BotStats : BaseCommandModule
{
    public MadsServiceProvider CommandService { get; set; }
    public IDbContextFactory<MadsContext> DbFactory { get; set; }

    [Command("botstats"), Aliases("bs", "stats"), Description("Get statistics about Mads")]
    public async Task GetBotStatsAsync(CommandContext ctx)
    {
        using var process = Process.GetCurrentProcess();

        var members = CommandService.ModularDiscordBot.DiscordClient.Guilds.Values.Select(x => x.MemberCount).Sum();
        var guilds = CommandService.ModularDiscordBot.DiscordClient.Guilds.Count;
        var ping = CommandService.ModularDiscordBot.DiscordClient.Ping;
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
        var heapMemory = $"{process.PrivateMemorySize64 / 1024 / 1024} MB";

        var embed = new DiscordEmbedBuilder();
        embed
            .WithTitle("Statistics")
            .WithColor(new DiscordColor(0, 255, 194))
            .AddField("Membercount:", members.ToString("N0"), true)
            .AddField("Guildcount:", guilds.ToString("N0"), true)
            .AddField("Ping:", ping.ToString("N0"), true)
            .AddField("Threads:", $"{ThreadPool.ThreadCount}", true)
            .AddField("Memory:", heapMemory, true)
            .AddField("Uptime:",
                $"{DateTimeOffset.UtcNow.Subtract(process.StartTime).Humanize(2, minUnit: TimeUnit.Millisecond, maxUnit: TimeUnit.Day)}",
                true);

        await ctx.RespondAsync(embed);
    }
}