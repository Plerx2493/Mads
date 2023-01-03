﻿using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MADS.CustomComponents;
using MADS.Extensions;
using MADS.Services;

namespace MADS.Commands.Slash;

public class MessageSnipe : MadsBaseApplicationCommand
{
    public VolatileMemoryService _memoryService;

    [SlashCommand("snipe", "Snipes the last deleted message.")]
    public Task SnipeAsync(InteractionContext ctx)
    {
        return DoSnipeAsync(ctx, false);
    }

    [SlashCommand("snipeedit", "Snipes the last edited message.")]
    public Task SnipeEditAsync(InteractionContext ctx)
    {
        return DoSnipeAsync(ctx, true);
    }

    private async Task DoSnipeAsync(InteractionContext ctx, bool edit)
    {
        DiscordMessage message;

        var result = edit switch
        {
            true => _memoryService.MessageSnipe.TryGetEditedMessage(ctx.Channel.Id, out message),
            false => _memoryService.MessageSnipe.TryGetMessage(ctx.Channel.Id, out message)
        };

        if (!result)
        {
            await ctx.CreateResponseAsync(
                "⚠️ No message to snipe! Either nothing was deleted, or the message has expired (12 hours)!", true);
            return;
        }

        var content = message.Content;
        if (content.Length > 500) content = content.Substring(0, 500) + "...";

        var embed = new DiscordEmbedBuilder()
                    .WithAuthor(
                        $"{message.Author.Username}#{message.Author.Discriminator}" + (edit ? " (Edited)" : ""),
                        iconUrl: message.Author.GetAvatarUrl(ImageFormat.Png))
                    .WithFooter(
                        $"{(edit ? "Edit" : "Deletion")} sniped by {ctx.User.Username}#{ctx.User.Discriminator}",
                        ctx.User.AvatarUrl);

        if (!string.IsNullOrEmpty(content))
        {
            embed.WithDescription(content);
        }

        embed.WithTimestamp(message.Id);

        var embeds = new List<DiscordEmbedBuilder>();
        var attachments = message.Attachments.Where(x => x.MediaType.StartsWith("image/")).ToList();


        for (var i = 0; i < attachments.Count(); i++)
        {
            var attachment = attachments.ElementAt(i);
            if (i == 0)
            {
                embed.WithThumbnail(attachment.Url);
            }
            else
            {
                embeds.Add(new DiscordEmbedBuilder()
                           .WithTitle("Additional Image").WithThumbnail(attachment.Url));
            }
        }

        var response = new DiscordInteractionResponseBuilder()
            .AddEmbeds(embeds.Prepend(embed).Select(x => x.Build()));
        
        DiscordButtonComponent btn = new DiscordButtonComponent(ButtonStyle.Danger, "test", "Delete (Author only)", emoji: new DiscordComponentEmoji("🗑"));
        btn = ActionDiscordButton.Build(ActionDiscordButtonEnum.DeleteOneUserOnly, btn, message.Author.Id);
        
        response.AddComponents(btn);

        await ctx.CreateResponseAsync(response);
    }
}