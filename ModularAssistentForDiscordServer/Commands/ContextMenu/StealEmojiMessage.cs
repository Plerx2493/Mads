﻿using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace MADS.Commands.ContextMenu;

public class StealEmojiMessage : ApplicationCommandModule
{
    private const string EmojiRegex = @"<a?:(.+?):(\d+)>";
    
    [ContextMenu(ApplicationCommandType.MessageContextMenu, "Steal emoji")]
    public async Task YoinkAsync(ContextMenuContext ctx)
    {
        var matches = Regex.Matches(ctx.TargetMessage.Content, EmojiRegex);
        
        if (matches.Count < 1)
        {
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder().WithTitle("⚠️ Emoji not found!"), true);
            return;
        }

        var split = matches[0].Groups[2].Value;
        var emojiName = matches[0].Groups[1].Value;
        var animated = matches[0].Value.StartsWith("<a");

        if (ulong.TryParse(split, out var emojiId))
        {
            await CopyEmoji(ctx, emojiName, emojiId, animated);
        }
        else
        {
            await ctx.CreateResponseAsync(new DiscordEmbedBuilder().WithTitle("⚠️ Failed to fetch your new emoji."), true);
        }
    }

    private static async Task CopyEmoji(ContextMenuContext ctx, string name, ulong id, bool animated)
    {
        using HttpClient httpClient = new();
        var downloadedEmoji =
            await httpClient.GetStreamAsync($"https://cdn.discordapp.com/emojis/{id}.{(animated ? "gif" : "png")}");
        MemoryStream memory = new();
        await downloadedEmoji.CopyToAsync(memory);
        await downloadedEmoji.DisposeAsync();
        var newEmoji = await ctx.Guild.CreateEmojiAsync(name, memory);
            
        await ctx.CreateResponseAsync(new DiscordEmbedBuilder().WithTitle($"✅ Yoink! This emoji has been added to your server: {newEmoji}"), true);
    }
}