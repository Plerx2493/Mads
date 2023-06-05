﻿// Copyright 2023 Plerx2493
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
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using MADS.Services;

namespace MADS.Commands.Slash;

public class MoveEmoji : MadsBaseApplicationCommand
{
    private const string EmojiRegex = @"<a?:(.+?):(\d+)>";

    [SlashCommand("MoveEmoji", "Move emoji to your guild"), SlashRequirePermissions(Permissions.ManageEmojis)]
    public async Task MoveEmojiAsync
        (InteractionContext ctx, [Option("Emoji", "Emoji which should be moved")] string pEmoji)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());

        var matches = Regex.Matches(pEmoji, EmojiRegex, RegexOptions.Compiled);

        if (!matches.Any())
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("There are no emojis in your input"));
            return;
        }

        var split = matches[0].Groups[2].Value;
        var emojiName = matches[0].Groups[1].Value;
        var animated = matches[0].Value.StartsWith("<a");

        if (!ulong.TryParse(split, out var emojiId))
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("⚠️ Failed to fetch your new emoji."));

        List<DiscordGuild> guilds = new();

        foreach (var guild in ctx.Client.Guilds.Values)
            try
            {
                var member = await guild.GetMemberAsync(ctx.User.Id);
                if (member.Permissions.HasFlag(Permissions.ManageEmojis)) guilds.Add(guild);
            }
            catch (DiscordException)
            {
            }

        if (!guilds.Any())
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("There are no guilds where you are able to add emojis"));
            return;
        }

        var options = guilds.Select(x => new DiscordSelectComponentOption(x.Name, x.Id.ToString())).ToList();

        //Create the select component and update our first response
        DiscordSelectComponent select = new("moveEmojiChooseGuild-" + ctx.User.Id,
            "Select guild", options, false, 0, options.Count());
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddComponents(select));

        //Get the initial response an wait for a component interaction
        var response = await ctx.GetOriginalResponseAsync();
        var selectResponse = await response.WaitForSelectAsync(ctx.Member, "moveEmojiChooseGuild-" + ctx.User.Id,
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
        await selectResponse.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("Submitted").AsEphemeral());
        await ctx.EditResponseAsync(new DiscordWebhookBuilder
        {
            Content = "Submitted"
        });

        foreach (var value in selectResponse.Result.Values)
        {
            var guildId = ulong.Parse(value);
            await CopyEmoji(ctx.Client, emojiName, emojiId, animated, guildId);
        }

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("End"));
    }

    private static async Task CopyEmoji(DiscordClient client, string name, ulong id, bool animated, ulong targetGuild)
    {
        using HttpClient httpClient = new();
        var downloadedEmoji =
            await httpClient.GetStreamAsync($"https://cdn.discordapp.com/emojis/{id}.{(animated ? "gif" : "png")}");
        MemoryStream memory = new();
        await downloadedEmoji.CopyToAsync(memory);
        await downloadedEmoji.DisposeAsync();
        var targetGuildEntity = await client.GetGuildAsync(targetGuild);
        var _ = await targetGuildEntity.CreateEmojiAsync(name, memory);
    }
}