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
using System.Text.RegularExpressions;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using MADS.Entities;
using MADS.Extensions;
using Microsoft.EntityFrameworkCore;

namespace MADS.Commands.Slash;

public sealed class StarboardConfig
{
    private static readonly Regex EmoteRegex = new(@"^<(?<animated>a)?:(?<name>[a-zA-Z0-9_]+?):(?<id>\d+?)>$",
        RegexOptions.ECMAScript | RegexOptions.Compiled);

    private IDbContextFactory<MadsContext> _contextFactory;

    public StarboardConfig(IDbContextFactory<MadsContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
    
    [Command("Starboard"), Description("Configure Starboard"),
     RequirePermissions(DiscordPermissions.ManageGuild),
     RequireGuild]
    public async Task StarboardConfigCommand
    (
        CommandContext ctx,
        [Description("Channel for starboard messages")]
        DiscordChannel channel,
        [Description("Emoji which is used as star (default: :star:)")]
        string emojiString = "⭐",
        [Description("Number of stars required for message (default: 3)")]
        long threshhold = 3
    )
    {
        await ctx.DeferAsync(true);

        MadsContext db = await _contextFactory.CreateDbContextAsync();

        GuildConfigDbEntity guildConfig = db.Configs.First(x => x.DiscordGuildId == ctx.Guild.Id);

        if (!DiscordEmoji.TryFromUnicode(emojiString, out DiscordEmoji emoji))
        {
            Match match = EmoteRegex.Match(emojiString);
            if (match.Success)
            {
                DiscordEmoji.TryFromGuildEmote(ctx.Client, ulong.Parse(match.Groups["id"].Value), out emoji);
            }
        }

        guildConfig.StarboardChannelId = channel.Id;
        guildConfig.StarboardThreshold = (int) threshhold;
        guildConfig.StarboardEmojiId = emoji.Id;
        guildConfig.StarboardEmojiName = emoji.Name;
        guildConfig.StarboardActive = true;

        db.Configs.Update(guildConfig);
        await db.SaveChangesAsync();
        await db.DisposeAsync();

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Success"));
    }
}