using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace MADS.Commands.Text.Base;

public class StealEmoji : BaseCommandModule
{
    private const string EmojiRegex = @"<a?:(.+?):(\d+)>";

    [Command("yoink"), Description("Copies an emoji from a different server to this one"),
     RequirePermissions(Permissions.ManageEmojis), Priority(1)]
    public async Task YoinkAsync(CommandContext ctx, DiscordEmoji emoji, [RemainingText] string name = "")
    {
        if (!emoji.ToString().StartsWith('<'))
        {
            await ctx.RespondAsync("⚠️ This is not a valid guild emoji!");
            return;
        }
        await CopyEmoji(ctx, string.IsNullOrEmpty(name) ? emoji.Name : name, emoji.Id, emoji.IsAnimated);
    }

    [Command("yoink"), RequirePermissions(Permissions.ManageEmojis), Priority(0)]
    public async Task YoinkAsync(CommandContext ctx, int index = 1)
    {
        if (ctx.Message.ReferencedMessage is null)
        {
            await ctx.RespondAsync("⚠️ You need to reply to an existing message to use this command!");
            return;
        }

        var matches = Regex.Matches(ctx.Message.ReferencedMessage.Content, EmojiRegex);
        if (matches.Count < index || index < 1)
        {
            await ctx.RespondAsync("⚠️ Emoji not found!");
            return;
        }

        var split = matches[index - 1].Groups[2].Value;
        var emojiName = matches[index - 1].Groups[1].Value;
        var animated = matches[index - 1].Value.StartsWith("<a");

        if (ulong.TryParse(split, out var emojiId))
        {
            await CopyEmoji(ctx, emojiName, emojiId, animated);
        }
        else
        {
            await ctx.RespondAsync("⚠️ Failed to fetch your new emoji.");
        }
    }

    private static async Task CopyEmoji(CommandContext ctx, string name, ulong id, bool animated)
    {
        using HttpClient httpClient = new();
        var downloadedEmoji =
            await httpClient.GetStreamAsync($"https://cdn.discordapp.com/emojis/{id}.{(animated ? "gif" : "png")}");
        MemoryStream memory = new();
        await downloadedEmoji.CopyToAsync(memory);
        await downloadedEmoji.DisposeAsync();
        var newEmoji = await ctx.Guild.CreateEmojiAsync(name, memory);
        await ctx.RespondAsync($"✅ Yoink! This emoji has been added to your server: {newEmoji}");
    }
}