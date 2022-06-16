using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MADS.Extensions
{
    public class LoggingProvider
    {
        //Utilities
        private readonly string dirPath = DataProvider.GetPath("Logs");
        private readonly DateTime startDate;
        private readonly string logPath;
        private readonly ModularDiscordBot modularDiscordBot;
        internal List<ulong> AdminChannel;

       

        internal LoggingProvider(ModularDiscordBot dBot)
        {
            startDate = DateTime.Now;



            modularDiscordBot = dBot;
            Directory.CreateDirectory(dirPath);

            logPath = DataProvider.GetPath("Logs", $"{startDate.Day}-{startDate.Month}-{startDate.Year}_{startDate.Hour}-{startDate.Minute}-{startDate.Second}.log");
            File.AppendAllTextAsync(logPath, "========== LOG START ==========\n\n", System.Text.Encoding.UTF8);
        }

        public Task LogEvent(string message, string sender, LogLevel lvl)
        {
            string log = $"[{DateTime.Now:yyyy'-'MM'-'dd'T'HH':'mm':'ss}] [{lvl}] [{sender}] {message}";
            File.AppendAllTextAsync(logPath, log + "\n", System.Text.Encoding.UTF8);
            return Task.CompletedTask;
        }

        public Task LogCommandExecution(CommandContext ctx, DiscordMessage response)
        {
            var triggerTime = ctx.Message.Timestamp.DateTime;
            var executionTime = response.Timestamp.DateTime;
            var Timespan = (executionTime - triggerTime).TotalMilliseconds;
            var Commandname = ctx.Command.Name;
            var CommandArgs = ctx.Command.CustomAttributes;

            string tmp = "";

            foreach (var arg in CommandArgs)
            {
                tmp += arg.ToString() + ", ";
            }

            tmp = tmp.Remove(tmp.Length - 2);

            var logEntry = $"[{DateTime.Now:dd'.'MM'.'yyyy'-'HH':'mm':'ss}] [INFO] [{ctx.User.Username}#{ctx.User.Discriminator} : {ctx.User.Id}] [{Commandname}] [{tmp}] {Timespan} milliseconds to execute";
            File.AppendAllTextAsync(logPath, logEntry + "\n", System.Text.Encoding.UTF8);

            return Task.CompletedTask;
        }

        public Task LogToAdmin(string message, string sender, LogLevel logLevel)
        {
            var logEntry = $"[{logLevel}] [{sender}] {message}";

            modularDiscordBot.DiscordClient.GetChannelAsync(AdminChannel[0]).Result.SendMessageAsync(logEntry);

            var DiscordEmbed = new DiscordEmbedBuilder
            {
                Title = "Log",
                Description = logEntry,
                Color = DiscordColor.Red,
                Timestamp = DateTime.Now,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "Logged by Mads"
                }
            };

            return Task.CompletedTask;
        }
    }
}
