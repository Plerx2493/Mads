using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using MADS.Extensions;

namespace MADS.Commands.ContextMenu;

public class StealEmojiMessage : MadsBaseApplicationCommand
{
    private const string EmojiRegex = @"<a?:(.+?):(\d+)>";

    [ContextMenu(ApplicationCommandType.MessageContextMenu, "Steal emoji(s)"),
     SlashRequirePermissions(Permissions.ManageEmojis)]
    public async Task YoinkAsync(ContextMenuContext ctx)
    {
        await ctx.DeferAsync(true);

        var matches = Regex.Matches(ctx.TargetMessage.Content.Replace("><","> <"), EmojiRegex);

        if (matches.Count < 1)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("⚠️ Emoji not found!"));
            return;
        }
        
        var newEmojis = new List<DiscordEmoji>();

        foreach (Match match in matches)
        {
            try
            {
                var split = match.Groups[2].Value;
                var emojiName = match.Groups[1].Value;
                var animated = match.Value.StartsWith("<a");

                if (!ulong.TryParse(split, out var emojiId))
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("⚠️ Failed to fetch your new emoji."));
                }
                var success = await CopyEmoji(ctx, emojiName, emojiId, animated);
            
                newEmojis.Add(success);
            }
            catch (Exception)
            {
                // ignored
            }
            await Task.Delay(1000);
        }
        
        var message = newEmojis.Aggregate("✅ Yoink! These emoji(s) have been added to your server: ", (current, emoji) => current + $" {emoji}");
        message += $" {newEmojis.Count}/{matches.Count} emojis added";
        
        var discordWebhook = new DiscordWebhookBuilder().AddEmbed(
            new DiscordEmbedBuilder().WithTitle(message));

        await ctx.EditResponseAsync(discordWebhook);
    }

    private static async Task<DiscordEmoji> CopyEmoji(ContextMenuContext ctx, string name, ulong id, bool animated)
    {
        using HttpClient httpClient = new();
        var downloadedEmoji =
            await httpClient.GetStreamAsync($"https://cdn.discordapp.com/emojis/{id}.{(animated ? "gif" : "png")}");

        MemoryStream memory = new();

        await downloadedEmoji.CopyToAsync(memory);

        await downloadedEmoji.DisposeAsync();
        var newEmoji = await ctx.Guild.CreateEmojiAsync(name, memory);
        
        return newEmoji;
    }
}