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
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Humanizer;

namespace MADS.Commands.Slash;

public sealed class Ping
{
    [Command("ping"), Description("Get the bot's ping")]
    public async Task PingCommand(CommandContext ctx)
    {
        using Process process = Process.GetCurrentProcess();

        DiscordEmbedBuilder discordEmbedBuilder = new();
        discordEmbedBuilder
            .WithTitle("Status")
            .WithTimestamp(DateTime.Now)
            .WithColor(new DiscordColor(0, 255, 194))
            .AddField("Uptime", $"{DateTimeOffset.UtcNow.Subtract(process.StartTime).Humanize(3, minUnit: TimeUnit.Millisecond, maxUnit: TimeUnit.Day)}")
            .AddField("Websocket ping", $"{ctx.Client.Ping} ms");
        
        DiscordInteractionResponseBuilder responseBuilder = new();
        responseBuilder.AddEmbed(discordEmbedBuilder).AsEphemeral();

        await ctx.RespondAsync(responseBuilder);
    }
}