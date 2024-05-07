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
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using MADS.Extensions;

namespace MADS.Commands.Slash;

public sealed partial class MoveEmoji
{
    [Command("MoveEmoji"), Description("Move emoji to your guild"), RequirePermissions(DiscordPermissions.ManageEmojis)]
    public async Task MoveEmojiAsync
        (CommandContext ctx, [Description("Emoji which should be moved")] string emoji)
    {
        await ctx.DeferAsync(true);

        MatchCollection matches = EmojiRegex().Matches(emoji);

        if (!matches.Any())
        {
            await ctx.EditResponse_Error("There are no emojis in your input");
            return;
        }

        string split = matches[0].Groups[2].Value;
        string emojiName = matches[0].Groups[1].Value;
        bool animated = matches[0].Value.StartsWith("<a");

        if (!ulong.TryParse(split, out ulong emojiId))
        {
            await ctx.EditResponse_Error("⚠️ Failed to fetch your new emoji.");
            return;
        }

        List<DiscordGuild> guilds = [];

        foreach (DiscordGuild guild in ctx.Client.Guilds.Values)
        {
            try
            {
                DiscordMember member = await guild.GetMemberAsync(ctx.User.Id);
                if (member.Permissions.HasFlag(DiscordPermissions.ManageEmojis))
                {
                    guilds.Add(guild);
                }
            }
            catch (DiscordException)
            {
            }
        }

        if (!guilds.Any())
        {
            await ctx.EditResponse_Error("There are no guilds where you are able to add emojis");
            return;
        }

        List<DiscordSelectComponentOption> options = guilds.Select(x => new DiscordSelectComponentOption(x.Name, x.Id.ToString())).ToList();

        //Create the select component and update our first response
        DiscordSelectComponent select = new("moveEmojiChooseGuild-" + ctx.User.Id,
            "Select guild", options, false, 0, options.Count());
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddComponents(select));

        //Get the initial response an wait for a component interaction
        DiscordMessage? response = await ctx.GetResponseAsync();
        InteractivityResult<ComponentInteractionCreateEventArgs> selectResponse = await response!.WaitForSelectAsync(ctx.Member, "moveEmojiChooseGuild-" + ctx.User.Id,
            TimeSpan.FromSeconds(60));

        //Notify the user when the interaction times out and abort
        if (selectResponse.TimedOut)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder
            {
                Content = "Timed out"
            });
            return;
        }

        //acknowledge interaction and edit first response to delete the select menu
        await selectResponse.Result.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("Submitted").AsEphemeral());
        await ctx.EditResponseAsync(new DiscordWebhookBuilder
        {
            Content = "Submitted"
        });

        foreach (string value in selectResponse.Result.Values)
        {
            ulong guildId = ulong.Parse(value);
            await CopyEmoji(ctx.Client, emojiName, emojiId, animated, guildId);
        }

        await ctx.EditResponse_Success("Emoji moved");
    }

    private static async Task CopyEmoji(DiscordClient client, string name, ulong id, bool animated, ulong targetGuild)
    {
        using HttpClient httpClient = new();
        Stream downloadedEmoji =
            await httpClient.GetStreamAsync($"https://cdn.discordapp.com/emojis/{id}.{(animated ? "gif" : "png")}");
        MemoryStream memory = new();
        await downloadedEmoji.CopyToAsync(memory);
        await downloadedEmoji.DisposeAsync();
        DiscordGuild targetGuildEntity = await client.GetGuildAsync(targetGuild);
        _ = await targetGuildEntity.CreateEmojiAsync(name, memory);
    }

    [GeneratedRegex("<a?:(.+?):(\\d+)>", RegexOptions.Compiled)]
    private static partial Regex EmojiRegex();
}