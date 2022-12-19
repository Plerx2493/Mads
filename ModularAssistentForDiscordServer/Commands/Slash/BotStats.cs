using System.Diagnostics;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Humanizer;
using Humanizer.Localisation;
using MADS.Extensions;

namespace MADS.Commands.Slash;

public class BotStats : MadsBaseApplicationCommand
{
    [SlashCommand("botstats", "Get statistics about the bot")]
    public async Task GetBotStatsAsync(InteractionContext ctx)
    {
        using var process = Process.GetCurrentProcess();

        var members = ctx.Client.Guilds.Values.Select(x => x.MemberCount).Sum();
        var guilds = ctx.Client.Guilds.Count;
        var ping = ctx.Client.Ping;
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

        await ctx.CreateResponseAsync(embed, true);
    }
}