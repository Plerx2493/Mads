using DSharpPlus;
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

        [Command("ping"), Aliases("status"), Description("Get the ping of the websocket")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task Ping(CommandContext ctx) 
        {
            try
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
            catch (Exception ex)
            {
                await CommandService.modularDiscordBot.Logging.LogEvent(ex.Message, ex.Source, LogLevel.Warning);
            }
        }
            
    }
}
