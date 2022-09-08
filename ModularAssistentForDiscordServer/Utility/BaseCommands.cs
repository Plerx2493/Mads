﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MADS.Extensions;
using MADS.JsonModel;
using MADS.Modules;
using System.Text.RegularExpressions;

namespace MADS.Utility
{
    internal class BaseCommands : BaseCommandModule
    {
        public MadsServiceProvider CommandService { get; set; }
        private const string EmojiRegex = @"<a?:(.+?):(\d+)>";

        [Command("ping"), Aliases("status"), Description("Get the ping of the websocket"), Cooldown(1, 30, CooldownBucketType.Channel)]
        public async Task Ping(CommandContext ctx)
        {
            var diff = DateTime.Now - CommandService.ModularDiscordBot.startTime;
            var date = $"{diff.Days} days {diff.Hours} hours {diff.Minutes} minutes";

            DiscordEmbedBuilder discordEmbedBuilder = GuildSettings.GetDiscordEmbed();
            discordEmbedBuilder
                .WithTitle("Status")
                .WithTimestamp(DateTime.Now)
                .AddField("Uptime", date)
                .AddField("Websocket ping", $"{ctx.Client.Ping} ms");

            var response = await ModularDiscordBot.AnswerWithDelete(ctx, discordEmbedBuilder.Build(), 20);
            CommandService.ModularDiscordBot.Logging.LogCommandExecutionAsync(ctx, response);
        }

        [Command("about"), Aliases("info"), Description("Displays a little information about this bot"), Cooldown(1, 30, CooldownBucketType.Channel)]
        public async Task About(CommandContext ctx)
        {
            var discordEmbedBuilder = GuildSettings.GetDiscordEmbed();
            var discordMessageBuilder = new DiscordMessageBuilder();
            string inviteUri = ctx.Client.CurrentApplication.GenerateOAuthUri(null, Permissions.Administrator, OAuthScope.Bot, OAuthScope.ApplicationsCommands);
            string addMe = $"[Click here!]({inviteUri.Replace(" ", "%20")})";

            var diff = DateTime.Now - CommandService.ModularDiscordBot.startTime;
            string date = $"{diff.Days} days {diff.Hours} hours {diff.Minutes} minutes";

            discordEmbedBuilder
                .WithTitle("About me")
                .WithDescription("A modular designed discord bot for moderation and stuff")
                .WithAuthor(ctx.Client.CurrentUser.Username, ctx.Client.CurrentUser.AvatarUrl, ctx.Client.CurrentUser.AvatarUrl)
                .AddField("Owner:", "[Plerx#0175](https://github.com/Plerx2493/)", true)
                .AddField("Source:", "[Github](https://github.com/Plerx2493/Mads)", true)
                .AddField("D#+ Version:", ctx.Client.VersionString)
                .AddField("Guilds", ctx.Client.Guilds.Count.ToString(), true)
                .AddField("Uptime", date, true)
                .AddField("Ping", $"{ctx.Client.Ping} ms", true)
                .AddField("Add me", addMe);

            discordMessageBuilder.AddEmbed(discordEmbedBuilder.Build());
            discordMessageBuilder.AddComponents(new DiscordButtonComponent(ButtonStyle.Success, "feedback-button", "Feedback"));

            var response = await ctx.RespondAsync(discordMessageBuilder);
            CommandService.ModularDiscordBot.Logging.LogCommandExecutionAsync(ctx, response);
        }

        [Command("prefix"), Description("Get the bot prefix for this server"), Cooldown(1, 30, CooldownBucketType.Channel)]
        public async Task GetPrefix(CommandContext ctx)
        {
            var allGuildSettings = CommandService.ModularDiscordBot.GuildSettings;

            if (!allGuildSettings.TryGetValue(ctx.Guild.Id, out GuildSettings guildSettings))
            {
                guildSettings = allGuildSettings[0];
            }

            await ctx.RespondAsync("Current prefix is: `" + guildSettings.Prefix + "`");
        }

        [Command("setprefix"), Description("Set the bot prefix for this server"), RequirePermissions(Permissions.Administrator), RequireGuild]
        public async Task SetPrefix(CommandContext ctx, [Description("The new prefix")] string prefix)
        {
            var allGuildSettings = CommandService.ModularDiscordBot.GuildSettings;

            if (!allGuildSettings.TryGetValue(ctx.Guild.Id, out GuildSettings guildSettings))
            {
                CommandService.ModularDiscordBot.GuildSettings.Add(ctx.Guild.Id, new GuildSettings() { Prefix = prefix });
            }
            else
            {
                guildSettings.Prefix = prefix;
                CommandService.ModularDiscordBot.GuildSettings[ctx.Guild.Id] = guildSettings;
            }

            DataProvider.SetConfig(CommandService.ModularDiscordBot.GuildSettings);

            ctx.Guild.CurrentMember.ModifyAsync(x => x.Nickname = ctx.Guild.CurrentMember.Username + $" [{prefix}]");

            await ctx.RespondAsync("New prefix is: " + $"`{CommandService.ModularDiscordBot.GuildSettings[ctx.Guild.Id].Prefix}`");
        }

        [Command("test"), RequireOwner]
        public async Task Test(CommandContext ctx)
        {
            var msg = await ctx.RespondAsync(embed: new DiscordEmbedBuilder()
                .WithColor(new DiscordColor("#FF007F"))
                .WithDescription("Test")
                .Build());

            await Task.Delay(2000);

            await msg.ModifyEmbedSuppressionAsync(true);

            await Task.Delay(2000);

            await msg.ModifyEmbedSuppressionAsync(false);
        }

        [Command("enable"), Description("Enable given module"), RequirePermissions(Permissions.Administrator), RequireGuild]
        public async Task EnableModule(CommandContext ctx, [Description("Name of new module")] string moduleName)
        {
            if (!CommandService.ModularDiscordBot.madsModules.TryGetValue(moduleName, out _))
            {
                await ctx.RespondAsync("Module not found");
                return;
            }

            var isEnabled = CommandService.ModularDiscordBot.GuildSettings[ctx.Guild.Id].AktivModules.Contains(moduleName);
            if (isEnabled)
            {
                await ctx.RespondAsync("Module already active");
                return;
            }

            CommandService.ModularDiscordBot.madsModules[moduleName].Enable(ctx.Guild.Id);
            await ctx.RespondAsync("Module is now enabled");
        }

        [Command("disable"), Description("Disable given module"), RequirePermissions(Permissions.Administrator), RequireGuild]
        public async Task DisableModule(CommandContext ctx, [Description("Name of module")] string moduleName)
        {
            if (!CommandService.ModularDiscordBot.madsModules.TryGetValue(moduleName, out _))
            {
                await ctx.RespondAsync("Module not found");
                return;
            }

            var isEnabled = CommandService.ModularDiscordBot.GuildSettings[ctx.Guild.Id].AktivModules.Contains(moduleName);
            if (!isEnabled)
            {
                await ctx.RespondAsync("Module was not active");
                return;
            }

            CommandService.ModularDiscordBot.madsModules[moduleName].Disable(ctx.Guild.Id);
            await ctx.RespondAsync("Module is now disabled");
        }

        [Command("modules"), Description("List all modules"), RequirePermissions(Permissions.Administrator), RequireGuild]
        public async Task ListModules(CommandContext ctx)
        {
            var modules = CommandService.ModularDiscordBot.madsModules.Keys.ToList();
            var response = modules.Aggregate("Available modules:\n", (current, module) => current + (module + "\n"));
            await ctx.RespondAsync(response);
        }

        [Command("modulesActive"), Description("Get active modules"), RequirePermissions(Permissions.Administrator), RequireGuild]
        public async Task ListActiveModules(CommandContext ctx)
        {
            var response = "";
            var tmp = CommandService.ModulesActivGuilds;

            tmp.ToList().ForEach(x =>
            {
                if (x.Value.Contains(ctx.Guild.Id))
                {
                    response += x.Key + "\n";
                }
            });

            if (response == "") await ctx.RespondAsync("No modules active");
            else await ctx.RespondAsync("Active Modules: \n" + response);
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

        [Command("yoink")]
        [Description("Copies an emoji from a different server to this one")]
        [RequirePermissions(Permissions.ManageEmojis), Priority(1)]
        public async Task YoinkAsync(CommandContext ctx, DiscordEmoji emoji, [RemainingText] string name = "")
        {
            if (!emoji.ToString().StartsWith('<'))
            {
                await ctx.RespondAsync("⚠️ This is not a valid guild emoji!");
                return;
            }
            await StealEmoji(ctx, string.IsNullOrEmpty(name) ? emoji.Name : name, emoji.Id, emoji.IsAnimated);
        }

        [Command("yoink")]
        [RequirePermissions(Permissions.ManageEmojis), Priority(0)]
        public async Task YoinkAsync(CommandContext ctx, int index = 1)
        {
            if (ctx.Message.ReferencedMessage is null)
            {
                await ctx.RespondAsync("⚠️ You need to reply to an existing message to use this command!");
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

            if (ulong.TryParse(split, out ulong emojiId))
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
            Stream downloadedEmoji = await httpClient.GetStreamAsync($"https://cdn.discordapp.com/emojis/{id}.{(animated ? "gif" : "png")}");
            MemoryStream memory = new();
            await downloadedEmoji.CopyToAsync(memory);
            await downloadedEmoji.DisposeAsync();
            var newEmoji = await ctx.Guild.CreateEmojiAsync(name, memory);
            await ctx.RespondAsync($"✅ Yoink! This emoji has been added to your server: {newEmoji}");
        }
    }
}