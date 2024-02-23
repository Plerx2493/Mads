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
using MADS.Extensions;

namespace MADS.Commands.ContextMenu;

public partial class StealEmojiMessage : MadsBaseApplicationCommand
{
    private readonly HttpClient _httpClient;
    
    public StealEmojiMessage(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    [ContextMenu(ApplicationCommandType.MessageContextMenu, "Steal emoji(s)"),
     SlashRequirePermissions(Permissions.ManageEmojis)]
    public async Task YoinkAsync(ContextMenuContext ctx)
    {
        await ctx.DeferAsync(true);
        
        if (ctx.TargetMessage.Content is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("⚠️ Message has not content!"));
            return;
        }

        MatchCollection matches = EmojiRegex().Matches(ctx.TargetMessage.Content.Replace("><", "> <"));

        if (matches.Count < 1)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("⚠️ Emoji not found!"));
            return;
        }

        List<Match> distinctMatches = matches.DistinctBy(x => x.Value).ToList();

        List<DiscordEmoji> newEmojis = new();

        foreach (Match match in distinctMatches)
        {
            try
            {
                string split = match.Groups[2].Value;
                string emojiName = match.Groups[1].Value;
                bool animated = match.Value.StartsWith("<a");

                if (!ulong.TryParse(split, out ulong emojiId))
                {
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().WithContent("⚠️ Failed to fetch your new emoji."));
                }

                DiscordEmoji success = await CopyEmoji(ctx, emojiName, emojiId, animated);

                newEmojis.Add(success);
            }
            catch (Exception)
            {
                // ignored
            }

            await IntendedWait(500);
        }

        string message = newEmojis.Aggregate("✅ Yoink! These emoji(s) have been added to your server: ",
            (current, emoji) => current + $" {emoji}");
        message += $" {newEmojis.Count}/{distinctMatches.Count} emojis added";

        DiscordWebhookBuilder discordWebhook = new DiscordWebhookBuilder().AddEmbed(
            new DiscordEmbedBuilder().WithTitle(message));

        await ctx.EditResponseAsync(discordWebhook);
    }

    private async Task<DiscordEmoji> CopyEmoji(ContextMenuContext ctx, string name, ulong id, bool animated)
    {
        Stream downloadedEmoji =
            await _httpClient.GetStreamAsync($"https://cdn.discordapp.com/emojis/{id}.{(animated ? "gif" : "png")}");

        MemoryStream memory = new();

        await downloadedEmoji.CopyToAsync(memory);

        await downloadedEmoji.DisposeAsync();
        DiscordGuildEmoji newEmoji = await ctx.Guild.CreateEmojiAsync(name, memory);

        return newEmoji;
    }

    [GeneratedRegex("<a?:(.+?):(\\d+)>", RegexOptions.Compiled)]
    private static partial Regex EmojiRegex();
}