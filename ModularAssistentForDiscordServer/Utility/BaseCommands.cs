using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MADS;
using MADS.Extensions;
using MADS.JsonModel;
using MADS.Modules;
using Microsoft.Extensions.Logging;
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

            DiscordEmbedBuilder discordEmbedBuilder = CommandService.modularDiscordBot.GuildSettings[0].DiscordEmbed;
            discordEmbedBuilder
                .WithTitle("Status")
                .WithTimestamp(DateTime.Now)
                .AddField("Uptime", date)
                .AddField("Websocket ping", $"{ctx.Client.Ping} ms");

            var response = await ModularDiscordBot.AnswerWithDelete(ctx, discordEmbedBuilder.Build(), 20);
            await CommandService.modularDiscordBot.Logging.LogCommandExecution(ctx, response);
        }

        [Command("about"), Aliases("info"), Description("Displays a little information about this bot"), Cooldown(1, 30, CooldownBucketType.Channel)]
        public async Task About(CommandContext ctx)
        {
            var discordEmbedBuilder = CommandService.modularDiscordBot.config.DiscordEmbed;
            string inviteUri = ctx.Client.CurrentApplication.GenerateOAuthUri(null, Permissions.Administrator, OAuthScope.Bot, OAuthScope.ApplicationsCommands);
            
            string addMe = $"[Click here!]({inviteUri.Replace(" ", "%20")})";

            discordEmbedBuilder
                .WithTitle("About me")
                .WithDescription("A modular desinged discord bot for moderation and stuff")
                .WithAuthor(ctx.Client.CurrentUser.Username, ctx.Client.CurrentUser.AvatarUrl, ctx.Client.CurrentUser.AvatarUrl)
                .AddField("Owner:", "[Plerx](https://github.com/Plerx2493/)")
                .AddField("Source:", "[Github](https://github.com/Plerx2493/Mads)", true)
                .AddField("D#+ Version:", ctx.Client.VersionString)
                .AddField("Guilds", ctx.Client.Guilds.Count.ToString(), true)
                .AddField("Add me", addMe);

            await ctx.RespondAsync(discordEmbedBuilder.Build());
        }

        [Command("prefix"), Description("Get the bot prefix for this server")]
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

        [Command("setprefix"), Description("Set the bot prefix for this server"), RequirePermissions(Permissions.Administrator), RequireGuild()]
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
             
            await ctx.RespondAsync("New prefix is: " + $"`{CommandService.modularDiscordBot.GuildSettings[ctx.Guild.Id].Prefix}`");
        }

        [Command("test"), RequireOwner()]
        public async Task Test(CommandContext ctx)
        {
            await CommandService.modularDiscordBot.DiscordClient.BulkOverwriteGlobalApplicationCommandsAsync(new List<DiscordApplicationCommand>());
            await ctx.RespondAsync("Done");
        }

        [Command("enable"), Description("Enable given module"),RequirePermissions(Permissions.Administrator), RequireGuild()]
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

        [Command("disable"), Description("Disable given module"), RequirePermissions(Permissions.Administrator), RequireGuild()]
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

        [Command("modules"), Description("List all modules"), RequirePermissions(Permissions.Administrator), RequireGuild()]
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

        [Command("modulesactiv"), Description("Get activ modules"), RequirePermissions(Permissions.Administrator), RequireGuild()]
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
        public async Task Exit(CommandContext ctx)
        {
            await ctx.RespondAsync("Leaving server...");
            await ctx.Guild.LeaveAsync();
        }

        [Command("leave"), Description("Leave given server"), RequireGuild, Hidden]
        public async Task LeaveGuildOwner(CommandContext ctx)
        {
            await ctx.Guild.LeaveAsync();
        }
    }
}
