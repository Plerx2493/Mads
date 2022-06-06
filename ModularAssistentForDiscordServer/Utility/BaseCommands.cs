﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MADS;
using MADS.Extensions;
using MADS.Modules;
using Microsoft.Extensions.Logging;

namespace ModularAssistentForDiscordServer.Utility
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
                .AddField("Ping", $"{ctx.Client.Ping} ms");

            var response = await ModularDiscordBot.AnswerWithDelete(ctx, discordEmbedBuilder.Build(), 20);
            await CommandService.modularDiscordBot.Logging.LogCommandExecution(ctx, response);
        }

        [Command("about"), Aliases("info"), Description("Displays a little information about this bot"), Cooldown(1, 30, CooldownBucketType.Channel)]
        public async Task About(CommandContext ctx)
        {
            var discordEmbedBuilder = CommandService.modularDiscordBot.config.DiscordEmbed;

            discordEmbedBuilder
                .WithTitle("About me")
                .WithDescription("A modular desinged discord bot for moderation and stuff")
                .WithAuthor(ctx.Client.CurrentUser.Username, ctx.Client.CurrentUser.AvatarUrl, ctx.Client.CurrentUser.AvatarUrl)
                .AddField("Owner:", "[Plerx](https://github.com/Plerx2493/)")
                .AddField("Source:", "[Github](https://github.com/Plerx2493/Mads)")
                .AddField("DSharpPlus Version:", ctx.Client.VersionString)
                .AddField("Guilds", ctx.Client.Guilds.Count.ToString());

            await ctx.RespondAsync(discordEmbedBuilder.Build());
        }
    }
}
