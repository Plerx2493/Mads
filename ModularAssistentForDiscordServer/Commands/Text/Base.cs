﻿using System.Diagnostics;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Humanizer;
using Humanizer.Localisation;
using MADS.CustomComponents;
using MADS.Entities;
using MADS.Extensions;
using Microsoft.EntityFrameworkCore;

namespace MADS.Commands.Text;

internal class BaseCommands : BaseCommandModule
{
    private const   string                         EmojiRegex = @"<a?:(.+?):(\d+)>";
    public          MadsServiceProvider            CommandService { get; set; }
    public          IDbContextFactory<MadsContext> _dbFactory     { get; set; }

    [Command("ping"), Aliases("status"), Description("Get the ping of the websocket"),
     Cooldown(1, 30, CooldownBucketType.Channel)]
    public async Task Ping(CommandContext ctx)
    {
        var diff = DateTime.Now - CommandService.ModularDiscordBot.startTime;
        var date = $"{diff.Days} days {diff.Hours} hours {diff.Minutes} minutes";

        var discordEmbedBuilder = CommandUtility.GetDiscordEmbed();
        discordEmbedBuilder
            .WithTitle("Status")
            .WithTimestamp(DateTime.Now)
            .AddField("Uptime", date)
            .AddField("Websocket ping", $"{ctx.Client.Ping} ms");

        var response = await ModularDiscordBot.AnswerWithDelete(ctx, discordEmbedBuilder.Build());
        await CommandService.ModularDiscordBot.Logging.LogCommandExecutionAsync(ctx, response);
    }

    [Command("about"), Aliases("info"), Description("Displays a little information about this bot"),
     Cooldown(1, 30, CooldownBucketType.Channel)]
    public async Task About(CommandContext ctx)
    {
        var discordEmbedBuilder = CommandUtility.GetDiscordEmbed();
        var discordMessageBuilder = new DiscordMessageBuilder();
        var inviteUri = ctx.Client.CurrentApplication.GenerateOAuthUri(null, Permissions.Administrator, OAuthScope.Bot,
            OAuthScope.ApplicationsCommands);
        var addMe = $"[Click here!]({inviteUri.Replace(" ", "%20")})";

        var diff = DateTime.Now - CommandService.ModularDiscordBot.startTime;
        var date = $"{diff.Days} days {diff.Hours} hours {diff.Minutes} minutes";

        discordEmbedBuilder
            .WithTitle("About me")
            .WithDescription("A modular designed discord bot for moderation and stuff")
            .WithAuthor(ctx.Client.CurrentUser.Username, ctx.Client.CurrentUser.AvatarUrl,
                ctx.Client.CurrentUser.AvatarUrl)
            .WithColor(new DiscordColor(0, 255, 194))
            .AddField("Owner:", "[Plerx#0175](https://github.com/Plerx2493/)", true)
            .AddField("Source:", "[Github](https://github.com/Plerx2493/Mads)", true)
            .AddField("D#+ Version:", ctx.Client.VersionString)
            .AddField("Guilds", ctx.Client.Guilds.Count.ToString(), true)
            .AddField("Uptime", date.Humanize(), true)
            .AddField("Ping", $"{ctx.Client.Ping} ms", true)
            .AddField("Add me", addMe);

        discordMessageBuilder.AddEmbed(discordEmbedBuilder.Build());
        discordMessageBuilder.AddComponents(new DiscordButtonComponent(ButtonStyle.Success, "feedback-button",
            "Feedback"));

        var response = await ctx.RespondAsync(discordMessageBuilder);
        await CommandService.ModularDiscordBot.Logging.LogCommandExecutionAsync(ctx, response);
    }

    [Command("prefix"), Description("Get the bot prefix for this server"), Cooldown(1, 30, CooldownBucketType.Channel), RequireGuild]
    public async Task GetPrefix(CommandContext ctx)
    {
        var dbContext = await _dbFactory.CreateDbContextAsync();
        GuildConfigDbEntity config;
        
        if (dbContext.Guilds.Any(x => x.Id == ctx.Guild.Id))
        {
           config = dbContext.Guilds.First(x => x.Id == ctx.Guild.Id).Config;
        }
        else
        {
            config = dbContext.Guilds.First(x => x.Id == 0).Config;
        }
        
        await ctx.RespondAsync("Current prefix is: `" + config.Prefix + "`");
    }
    
    [Command("setprefix"), Description("Set the bot prefix for this server"),
     RequirePermissions(Permissions.Administrator), RequireGuild]
    public async Task SetPrefix(CommandContext ctx, [Description("The new prefix")] string prefix)
    {
        var dbContext = await _dbFactory.CreateDbContextAsync();
        GuildConfigDbEntity config;
        
        if (dbContext.Guilds.Any(x => x.Id == ctx.Guild.Id))
        {
            config = dbContext.Guilds.First(x => x.Id == ctx.Guild.Id).Config;
        }
        else
        {
            config = dbContext.Guilds.First(x => x.Id == 0).Config;
        }

        config.Prefix = prefix;
        
        dbContext.SaveChanges();

        await ctx.Guild.CurrentMember.ModifyAsync(x => x.Nickname = ctx.Guild.CurrentMember.Username + $" [{prefix}]");

        await ctx.RespondAsync($"New prefix is: `{prefix}`");
    }

    [Command("jumppad"), Aliases("jp"), RequireGuild, RequireUserPermissions(permissions: Permissions.MoveMembers)]
    public async Task Test(CommandContext ctx, ulong originChannel, ulong targetChannel)
    {
        DiscordMessageBuilder message = new();
        DiscordButtonComponent newButton = new(ButtonStyle.Success, "test", "Hüpf");
        var actionButton = ActionDiscordButton.Build(ActionDiscordButtonEnum.MoveVoiceChannel, newButton, originChannel,
            targetChannel);

        message.AddComponents(actionButton);
        message.Content = "Jumppad";
        await ctx.RespondAsync(message);
    }

    
    [Command("exit"), Description("Exit the bot"), RequirePermissions(Permissions.Administrator), RequireGuild]
    public static async Task ExitGuild(CommandContext ctx)
    {
        await ctx.RespondAsync("Leaving server...");
        await ctx.Guild.LeaveAsync();
    }

    [Command("leave"), Description("Leave given server"), RequireGuild, Hidden, RequireOwner]
    public static async Task LeaveGuildOwner(CommandContext ctx)
    {
        await ctx.Guild.LeaveAsync();
    }

    [Command("yoink"), Description("Copies an emoji from a different server to this one"),
     RequirePermissions(Permissions.ManageEmojis), Priority(1)]
    public async Task YoinkAsync(CommandContext ctx, DiscordEmoji emoji, [RemainingText] string name = "")
    {
        if (!emoji.ToString().StartsWith('<'))
        {
            await ctx.RespondAsync("⚠️ This is not a valid guild emoji!");
            return;
        }
        await StealEmoji(ctx, string.IsNullOrEmpty(name) ? emoji.Name : name, emoji.Id, emoji.IsAnimated);
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
            await StealEmoji(ctx, emojiName, emojiId, animated);
        }
        else
        {
            await ctx.RespondAsync("⚠️ Failed to fetch your new emoji.");
        }
    }

    private static async Task StealEmoji(CommandContext ctx, string name, ulong id, bool animated)
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

    [Command("botstats"), Aliases("bs", "stats"), Description("Get statistics about Mads")]
    public async Task GetBotStatsAsync(CommandContext ctx)
    {
        using var process = Process.GetCurrentProcess();

        var members = CommandService.ModularDiscordBot.DiscordClient.Guilds.Values.Select(x => x.MemberCount).Sum();
        var guilds = CommandService.ModularDiscordBot.DiscordClient.Guilds.Count;
        var ping = CommandService.ModularDiscordBot.DiscordClient.Ping;
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
        var heapMemory = $"{process.PrivateMemorySize64 / 1024 / 1024} MB";

        var embed = new DiscordEmbedBuilder();
        embed
            .WithTitle("Statistics")
            .WithColor(new DiscordColor(0, 255, 194))
            .AddField("Membercount:", members.ToString("N0"), true)
            .AddField("Guildcount:", guilds.ToString("N0"), true)
            .AddField("Ping:", ping.ToString("N0"), true)
            .AddField("Threads:", $"{ThreadPool.ThreadCount}", true)
            .AddField("Memory:", heapMemory, true)
            .AddField("Uptime:",
                $"{DateTimeOffset.UtcNow.Subtract(process.StartTime).Humanize(2, minUnit: TimeUnit.Millisecond, maxUnit: TimeUnit.Day)}",
                true);

        await ctx.RespondAsync(embed);
    }

    [Command("user"), Aliases("userinfo", "stalking")]
    public async Task GetUserInfo(CommandContext ctx, DiscordUser user = null)
    {
        DiscordMember member = null;

        user ??= ctx.User;
        try
        {
            if (!ctx.Channel.IsPrivate)
            {
                member = await ctx.Guild.GetMemberAsync(user.Id);
            }
        }
        catch (DiscordException e)
        {
            if (e.GetType() != typeof(NotFoundException))
            {
                throw;
            }
        }

        DiscordEmbedBuilder embed = new();

        embed
            .WithAuthor($"{user.Username}#{user.Discriminator}", null, user.AvatarUrl)
            .WithColor(new DiscordColor(0, 255, 194))
            .AddField("Creation:",
                $"{user.CreationTimestamp.Humanize()} {Formatter.Timestamp(user.CreationTimestamp, TimestampFormat.ShortDate)}",
                true)
            .AddField("ID:", user.Id.ToString(), true);

        if (member is not null)
        {
            embed.AddField("Joined at:",
                $"{member.JoinedAt.Humanize()} {Formatter.Timestamp(member.JoinedAt, TimestampFormat.ShortDate)}",
                true);
            if (member.MfaEnabled.HasValue)
            {
                embed.AddField("2FA:", member.MfaEnabled.ToString());
            }

            embed
                .AddField("Permissions:", member.Permissions.Humanize())
                .AddField("Hierarchy:", member.Hierarchy.ToString(), true);
            if (member.Roles.Any())
            {
                embed.AddField("Roles", member.Roles.Select(x => x.Name).Humanize());
            }
        }

        await ctx.RespondAsync(embed.Build());
    }
}