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