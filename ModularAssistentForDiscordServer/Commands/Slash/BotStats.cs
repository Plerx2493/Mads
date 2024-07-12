// Copyright 2023 Plerx2493
//
// Licensed under the Apache License, Version 2.0 (the "License")
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.ComponentModel;
using System.Diagnostics;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Humanizer;
using MADS.Entities;
using MADS.Extensions;
using Microsoft.EntityFrameworkCore;

namespace MADS.Commands.Slash;

public sealed class BotStats
{
    private readonly IDbContextFactory<MadsContext> _contextFactory;
    private readonly DiscordRestClient _discordRestClient;

    public BotStats(IDbContextFactory<MadsContext> contextFactory, DiscordRestClient discordRestClient)
    {
        _contextFactory = contextFactory;
        _discordRestClient = discordRestClient;
    }
    
    [Command("botstats"), Description("Get statistics about the bot")]
    public async Task GetBotStatsAsync(CommandContext ctx)
    {
        await ctx.DeferAsync(true);
        await using MadsContext db = await _contextFactory.CreateDbContextAsync();
        Stopwatch swDb = new();
        Stopwatch swRest = new();

        _ = await db.Users.FirstOrDefaultAsync();
        swDb.Start();
        _ = await db.Guilds.FirstOrDefaultAsync();
        swDb.Stop();

        _ = await _discordRestClient.GetChannelAsync(ctx.Channel.Id);
        swRest.Start();
        _ = await _discordRestClient.GetUserAsync(ctx.User.Id);
        swRest.Stop();

        using Process process = Process.GetCurrentProcess();

        int members = db.Users.Count();
        int guilds = db.Guilds.Count();
        TimeSpan ping = ctx.Client.GetConnectionLatency(0);
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
        string heapMemory = $"{process.PrivateMemorySize64 / 1024 / 1024} MB";

        DiscordEmbedBuilder embed = new();
        embed
            .WithTitle("Statistics")
            .WithColor(new DiscordColor(0, 255, 194))
            .AddField("Membercount:", members.ToString("N0"), true)
            .AddField("Guildcount:", guilds.ToString("N0"), true)
            .AddField("Threads:", $"{ThreadPool.ThreadCount}", true)
            .AddField("Websocket Latency (Shard 0):", ping.TotalMilliseconds.ToString("N0") + " ms", true)
            .AddField("DB Latency:", swDb.ElapsedMilliseconds.ToString("N0") + " ms", true)
            .AddField("Rest Latency:", swRest.ElapsedMilliseconds.ToString("N0") + " ms", true)
            .AddField("Memory:", heapMemory, true)
            .AddField("Uptime:",
                $"{DateTimeOffset.UtcNow.Subtract(process.StartTime).Humanize(3, minUnit: TimeUnit.Millisecond, maxUnit: TimeUnit.Day)}",
                true);
        
        DiscordInteractionResponseBuilder responseBuilder = new();
        responseBuilder.AddEmbed(embed).AsEphemeral();

        await ctx.RespondAsync(responseBuilder);
    }
}