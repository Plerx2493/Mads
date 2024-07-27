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
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using MADS.Extensions;

namespace MADS.Commands.Slash;

public sealed class Purge
{
    [Command("purge"), Description("Purges messages"),
     RequirePermissions(DiscordPermissions.ManageMessages),
     RequireGuild]
    public async Task PurgeMessages
    (
        CommandContext ctx, [Description("Amount of messages to delete")] long amount = 100
    )
    {
        if (amount > 100)
        {
            await ctx.RespondErrorAsync("You cannot purge more than 100 messages at once", true);
            return;
        }

        await ctx.DeferAsync();
        DiscordMessage? response = await ctx.GetResponseAsync()!;

        List<DiscordMessage> messages = [];
        await foreach (DiscordMessage msg in ctx.Channel.GetMessagesAsync((int)amount))
        {
            messages.Add(msg);
        }

        messages.RemoveAll(x => (DateTime.UtcNow - x.Timestamp).TotalDays >= 14);
        messages.Remove(response!);

        await ctx.Channel.DeleteMessagesAsync(messages);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{messages.Count} messages deleted"));

        await Task.Delay(10_000);

        await ctx.DeleteResponseAsync();
    }
}