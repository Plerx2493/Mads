using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MADS;
using MADS.Extensions;
using MADS.JsonModel;
using MADS.Modules;
using MADS.Entities;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Encodings.Web;

namespace MADS.Utility
{
    internal class BaseCommands : BaseCommandModule
    {
        public MadsServiceProvider CommandService { get; set; }

        [Command("ping"), Aliases("status"), Description("Get the ping of the websocket"), Cooldown(1, 30, CooldownBucketType.Channel)]
        public async Task Ping(CommandContext ctx)
        {
            var diff = DateTime.Now - CommandService.modularDiscordBot.startTime;
            var date = string.Format("{0} days {1} hours {2} minutes", diff.Days, diff.Hours, diff.Minutes);

            DiscordEmbedBuilder discordEmbedBuilder = CommandService.modularDiscordBot.GuildSettings[0].GetDiscordEmbed();
            discordEmbedBuilder
                .WithTitle("Status")
                .WithTimestamp(DateTime.Now)
                .AddField("Uptime", date)
                .AddField("Websocket ping", $"{ctx.Client.Ping} ms");

            var response = await ModularDiscordBot.AnswerWithDelete(ctx, discordEmbedBuilder.Build(), 20);
            CommandService.modularDiscordBot.Logging.LogCommandExecutionAsync(ctx, response);
        }

        [Command("about"), Aliases("info"), Description("Displays a little information about this bot"), Cooldown(1, 30, CooldownBucketType.Channel)]
        public async Task About(CommandContext ctx)
        {
            var discordEmbedBuilder = CommandService.modularDiscordBot.config.GuildSettings[0].GetDiscordEmbed();
            var discordMessageBuilder = new DiscordMessageBuilder();
            string inviteUri = ctx.Client.CurrentApplication.GenerateOAuthUri(null, Permissions.Administrator, OAuthScope.Bot, OAuthScope.ApplicationsCommands);
            string addMe = $"[Click here!]({inviteUri.Replace(" ", "%20")})";

            var diff = DateTime.Now - CommandService.modularDiscordBot.startTime;
            string date = string.Format("{0} days {1} hours {2} minutes", diff.Days, diff.Hours, diff.Minutes);
            

            discordEmbedBuilder
                .WithTitle("About me")
                .WithDescription("A modular desinged discord bot for moderation and stuff")
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
            CommandService.modularDiscordBot.Logging.LogCommandExecutionAsync(ctx, response);
        }

        [Command("prefix"), Description("Get the bot prefix for this server"), Cooldown(1, 30, CooldownBucketType.Channel)]
        public async Task GetPrefix(CommandContext ctx)
        {
            GuildSettings guildSettings;
            var allGuildSettings = CommandService.modularDiscordBot.GuildSettings;

            if (ctx.Guild is not null)
            {
                if (!allGuildSettings.TryGetValue(ctx.Guild.Id, out guildSettings))
                {
                    guildSettings = allGuildSettings[0];
                }
            }
            else
            {
                guildSettings = allGuildSettings[0];
            }

            await ctx.RespondAsync("Current prefix is: `" + guildSettings.Prefix + "`");
        }

        [Command("setprefix"), Description("Set the bot prefix for this server"), RequirePermissions(Permissions.Administrator), RequireGuild]
        public async Task SetPrefix(CommandContext ctx, [Description("The new prefix")] string prefix)
        {
            var allGuildSettings = CommandService.modularDiscordBot.GuildSettings;

            if (!allGuildSettings.TryGetValue(ctx.Guild.Id, out GuildSettings guildSettings))
            {
                CommandService.modularDiscordBot.GuildSettings.Add(ctx.Guild.Id, new GuildSettings() { Prefix = prefix});
            }
            else
            {
                guildSettings.Prefix = prefix;
                CommandService.modularDiscordBot.GuildSettings[ctx.Guild.Id] = guildSettings;
            }

            DataProvider.SetConfig(CommandService.modularDiscordBot.GuildSettings);

            ctx.Guild.CurrentMember.ModifyAsync(x => x.Nickname = ctx.Guild.CurrentMember.Username + $" [{prefix}]");

            await ctx.RespondAsync("New prefix is: " + $"`{CommandService.modularDiscordBot.GuildSettings[ctx.Guild.Id].Prefix}`");
        }

        [Command("test"), RequireOwner]
        public static async Task Test(CommandContext ctx, DiscordChannel ch1, DiscordChannel ch2)
        {
            var msg = new DiscordMessageBuilder();
            var button = ActionDiscordButton.Build(ActionDiscordButtonEnum.MoveVoiceChannel, new DiscordButtonComponent(ButtonStyle.Danger, "", "ID"), ch1.Id, ch2.Id);

            msg.AddComponents(button);
            msg.Content = "Hüpf";
            
            await ctx.Channel.SendMessageAsync(msg);
        }

        [Command("enable"), Description("Enable given module"),RequirePermissions(Permissions.Administrator), RequireGuild]
        public async Task EnableModule(CommandContext ctx, [Description("Name of new module")] string moduleName)
        {
            if (!CommandService.modularDiscordBot.madsModules.TryGetValue(moduleName, out _))
            {
                await ctx.RespondAsync("Module not found");
                return;
            }
            
            CommandService.modularDiscordBot.madsModules[moduleName].Enable(ctx.Guild.Id);
            await ctx.RespondAsync("Module is now enabled");
            return;
        }

        [Command("disable"), Description("Disable given module"), RequirePermissions(Permissions.Administrator), RequireGuild]
        public async Task DisableModule(CommandContext ctx, [Description("Name of module")] string moduleName)
        {
            if (!CommandService.modularDiscordBot.madsModules.TryGetValue(moduleName, out _))
            {
                await ctx.RespondAsync("Module not found");
                return;
            }

            CommandService.modularDiscordBot.madsModules[moduleName].Disable(ctx.Guild.Id);
            await ctx.RespondAsync("Module is now disabled");
            return;
        }

        [Command("modules"), Description("List all modules"), RequirePermissions(Permissions.Administrator), RequireGuild]
        public async Task ListModules(CommandContext ctx)
        {
            var modules = CommandService.modularDiscordBot.madsModules.Keys.ToList();
            var response = "";
            foreach (var module in modules)
            {
                response += module + "\n";
            }
            await ctx.RespondAsync(response);
        }

        [Command("modulesactiv"), Description("Get activ modules"), RequirePermissions(Permissions.Administrator), RequireGuild]
        public async Task ListActiveModules(CommandContext ctx)
        {
            var response = "";
            var tmp = CommandService.modulesActivGuilds;

            tmp.ToList().ForEach(x =>
            {
                if (x.Value.Contains(ctx.Guild.Id))
                {
                    response += x.Key + "\n";
                }
            });

            if (response == "") await ctx.RespondAsync("No modules active");
            else await ctx.RespondAsync("Activ Modules: \n" + response);
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
    }
}
