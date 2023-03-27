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

using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using MADS.Entities;
using MADS.Extensions;
using Microsoft.EntityFrameworkCore;

namespace MADS.Commands.Slash;

public class StarboardConfig : MadsBaseApplicationCommand
{
    public IDbContextFactory<MadsContext> ContextFactory { get; set; }

    private static readonly Regex EmoteRegex = new Regex(@"^<(?<animated>a)?:(?<name>[a-zA-Z0-9_]+?):(?<id>\d+?)>$",
        RegexOptions.ECMAScript | RegexOptions.Compiled);


    [SlashCommand("Starboard", "Configure Starboard"),
     SlashRequirePermissions(Permissions.ManageGuild),
     SlashRequireGuild]
    public async Task StarboardConfigCommand
    (
        InteractionContext ctx,
        [Option("Channel", "Channel for starboard messages")]
        DiscordChannel channel,
        [Option("Emoji", "Emoji which is used as star (default: :star:)")]
        string emojiString = "⭐",
        [Option("Threshold", "Number of stars required for message (default: 3)")]
        long threshhold = 3
    )
    {
        await ctx.DeferAsync(true);

        var db = await ContextFactory.CreateDbContextAsync();

        var guildConfig = db.Configs.First(x => x.DiscordGuildId == ctx.Guild.Id);

        if (!DiscordEmoji.TryFromUnicode(emojiString, out var emoji))
        {
            var match = EmoteRegex.Match(emojiString);
            if (match.Success)
            {
                DiscordEmoji.TryFromGuildEmote(ctx.Client, ulong.Parse(match.Groups["id"].Value), out emoji);
            }
        }

        guildConfig.StarboardChannelId = channel.Id;
        guildConfig.StarboardThreshold = (int) threshhold;
        guildConfig.StarboardEmojiId = (ulong?) emoji.Id ?? 0;
        guildConfig.StarboardEmojiName = emoji.Name;
        guildConfig.StarboardActive = true;

        db.Configs.Update(guildConfig);
        await db.SaveChangesAsync();
        await db.DisposeAsync();

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Success"));
    }
}