using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace MADS.Commands.ContextMenu;

public class StealEmojiMessage : ApplicationCommandModule
{
    private const string EmojiRegex = @"<a?:(.+?):(\d+)>";
    
    [ContextMenu(ApplicationCommandType.MessageContextMenu, "Steal emoji"),
    SlashRequirePermissions(Permissions.ManageEmojis)]
    public async Task YoinkAsync(ContextMenuContext ctx)
    {
        await ctx.DeferAsync(true);
        
        var matches = Regex.Matches(ctx.TargetMessage.Content, EmojiRegex);
        
        if (matches.Count < 1)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("⚠️ Emoji not found!"));
            return;
        }

        var split = matches[0].Groups[2].Value;
        var emojiName = matches[0].Groups[1].Value;
        var animated = matches[0].Value.StartsWith("<a");

        if (!ulong.TryParse(split, out var emojiId))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("⚠️ Failed to fetch your new emoji."));
        }
        var success = await CopyEmoji(ctx, emojiName, emojiId, animated);

        if (!success)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("⚠️ Failed to fetch your new emoji."));
        }
    }

    private static async Task<bool> CopyEmoji(ContextMenuContext ctx, string name, ulong id, bool animated)
    {
        using HttpClient httpClient = new();
        var downloadedEmoji = await httpClient.GetStreamAsync($"https://cdn.discordapp.com/emojis/{id}.{(animated ? "gif" : "png")}");
        
        MemoryStream memory = new();
        
        await downloadedEmoji.CopyToAsync(memory);
        
        await downloadedEmoji.DisposeAsync();
        var newEmoji = await ctx.Guild.CreateEmojiAsync(name, memory);
            
        var discordWebhook = new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder().WithTitle($"✅ Yoink! This emoji has been added to your server: {newEmoji}"));
        
        await ctx.EditResponseAsync(discordWebhook);
        return true;
    }
}