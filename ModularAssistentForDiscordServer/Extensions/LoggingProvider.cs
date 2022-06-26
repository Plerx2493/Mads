using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
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
        private readonly List<DiscordDmChannel> OwnerChannel = new();

        internal LoggingProvider(ModularDiscordBot dBot)
        {
            startDate = DateTime.Now;
            modularDiscordBot = dBot;
            Directory.CreateDirectory(dirPath);

            logPath = DataProvider.GetPath("Logs", $"{startDate.Day}-{startDate.Month}-{startDate.Year}_{startDate.Hour}-{startDate.Minute}-{startDate.Second}.log");
            File.AppendAllTextAsync(logPath, "========== LOG START ==========\n\n", System.Text.Encoding.UTF8);

        }

        public async void Setup()
        {
            var application = modularDiscordBot.DiscordClient.CurrentApplication;
            var owners = application.Owners;
            var guilds = modularDiscordBot.DiscordClient.Guilds.Values;

            foreach (DiscordUser owner in owners)
            {
                DiscordMember member = null;

                foreach (DiscordGuild guild in guilds)
                {
                    try
                    {
                        member = await guild.GetMemberAsync(owner.Id);
                    }
                    catch (DiscordException)
                    {
                        continue;
                    }
                    break;
                }

                if (member is not null)
                {
                    var channel = await member.CreateDmChannelAsync();
                    OwnerChannel.Add(channel);
                }
            }

            Console.WriteLine(OwnerChannel.Count);
        }
        

        public Task LogEvent(string message, string sender, LogLevel lvl)
        {
            string log = $"[{DateTime.Now:yyyy'-'MM'-'dd'T'HH':'mm':'ss}] [{lvl}] [{sender}] {message}";
            File.AppendAllTextAsync(logPath, log + "\n", System.Text.Encoding.UTF8);
            return Task.CompletedTask;
        }

        public async Task LogCommandExecutionAsync(CommandContext ctx, DiscordMessage response)
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
            await File.AppendAllTextAsync(logPath, logEntry + "\n", System.Text.Encoding.UTF8);
        }
        
        public async Task LogToOwner(string message, string sender, LogLevel logLevel)
        {
            var discordEmbed = new DiscordEmbedBuilder
            {
                Title = logLevel.ToString(),
                Description = message,
                Color = new(new(0, 255, 194)),
                Timestamp = DateTime.Now,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "Send by " + sender
                }
            };

            foreach (DiscordDmChannel channel in OwnerChannel)
            {
                await channel.SendMessageAsync(discordEmbed);
            }
        }
        
    }
}
