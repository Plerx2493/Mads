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

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MADS.CustomComponents;
using MADS.Extensions;
using MADS.Services;

namespace MADS.Commands.Slash;

public sealed class MessageSnipe : MadsBaseApplicationCommand
{
    private readonly MessageSnipeService _messageSnipeService;

    public MessageSnipe(MessageSnipeService messageSnipeService)
    {
        _messageSnipeService = messageSnipeService;
    }
    
    [SlashCommand("snipe", "Snipes the last deleted message.")]
    public async Task SnipeAsync(InteractionContext ctx)
    {
        await DoSnipeAsync(ctx, false);
    }

    [SlashCommand("snipeedit", "Snipes the last edited message.")]
    public async Task SnipeEditAsync(InteractionContext ctx)
    {
        await DoSnipeAsync(ctx, true);
    }

    private async Task DoSnipeAsync(InteractionContext ctx, bool edit)
    {
        await ctx.DeferAsync(true);

        bool result = !edit
            ? _messageSnipeService.TryGetMessage(ctx.Channel.Id, out DiscordMessage? message)
            : _messageSnipeService.TryGetEditedMessage(ctx.Channel.Id, out message);

        if (!result || message is null)
        {
            await EditResponse_Error("⚠️ No message to snipe! Either nothing was deleted, or the message has expired (12 hours)!");
            return;
        }

        string? content = message.Content;
        if (content is not null && content.Length > 500)
        {
            content = string.Concat(content.AsSpan(0, 497), "...");
        }

        DiscordMember? member = message.Author is not null ? await ctx.Guild.GetMemberAsync(message.Author.Id) : null;

        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithAuthor(
                $"{member?.DisplayName ?? message.Author?.GlobalName}" + (edit ? " (Edited)" : ""),
                iconUrl: message.Author?.GetAvatarUrl(ImageFormat.Png))
            .WithFooter(
                $"{(edit ? "Edit" : "Deletion")} sniped by {ctx.Member.DisplayName}",
                ctx.User.AvatarUrl);

        if (!string.IsNullOrEmpty(content))
        {
            embed.WithDescription(content);
        }

        embed.WithTimestamp(message.Id);

        List<DiscordEmbedBuilder> embeds = [];
        List<DiscordAttachment> attachments = message.Attachments.Where(x => x.MediaType?.StartsWith("image/") ?? false).ToList();


        for (int i = 0; i < attachments.Count; i++)
        {
            DiscordAttachment attachment = attachments.ElementAt(i);
            if (i == 0 && attachment.Url is not null)
            {
                embed.WithThumbnail(attachment.Url);
            }
            else if (attachment.Url is not null)
            {
                embeds.Add(new DiscordEmbedBuilder()
                    .WithTitle("Additional Image").WithThumbnail(attachment.Url));
            }
        }

        DiscordWebhookBuilder response = new DiscordWebhookBuilder()
            .AddEmbeds(embeds.Prepend(embed).Select(x => x.Build()));

        DiscordButtonComponent btn = new(DiscordButtonStyle.Danger, "placeholder", "Delete (Author only)",
            emoji: new DiscordComponentEmoji("🗑"));
        btn = btn.AsActionButton(ActionDiscordButtonEnum.DeleteOneUserOnly, message.Author!.Id);

        response.AddComponents(btn);

        if (edit)
        {
            DiscordLinkButtonComponent btn1 = new(message.JumpLink.ToString(), "Go to message");
            response.AddComponents(btn1);
        }

        await ctx.EditResponseAsync(response);
    }
    
    [SlashCommand("deletesnipe", "Deletes cached messages for this channel.")]
    public async Task DeleteSnipeAsync(InteractionContext ctx)
    {
        await ctx.DeferAsync(true);
        _messageSnipeService.DeleteMessage(ctx.Channel.Id);
        _messageSnipeService.DeleteEditedMessage(ctx.Channel.Id);
        await EditResponse_Success("✅ Snipe cache cleared!");
    }
}