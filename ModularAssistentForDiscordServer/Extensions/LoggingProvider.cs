using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;

namespace MADS.Extensions
{
    public class LoggingProvider
    {
        //Utilities
        private readonly string _dirPath = DataProvider.GetPath("Logs");

        private readonly DateTime _startDate;
        private readonly string _logPath;
        private readonly ModularDiscordBot _modularDiscordBot;
        private readonly List<DiscordDmChannel> _ownerChannel = new();

        internal LoggingProvider(ModularDiscordBot dBot)
        {
            _startDate = DateTime.Now;
            _modularDiscordBot = dBot;
            Directory.CreateDirectory(_dirPath);

            _logPath = DataProvider.GetPath("Logs", $"{_startDate.Day}-{_startDate.Month}-{_startDate.Year}_{_startDate.Hour}-{_startDate.Minute}-{_startDate.Second}.log");
            File.AppendAllTextAsync(_logPath, "========== LOG START ==========\n\n", System.Text.Encoding.UTF8);
        }

        public async void Setup()
        {
            PopulateOwnerChannel();
            SetupFeedback();
        }

        private async void PopulateOwnerChannel()
        {
            var application = _modularDiscordBot.DiscordClient.CurrentApplication;
            var owners = application.Owners.ToArray();
            var guilds = _modularDiscordBot.DiscordClient.Guilds.Values.ToArray();

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

                if (member is null)
                {
                    continue;
                }

                var channel = await member.CreateDmChannelAsync();
                _ownerChannel.Add(channel);
            }

            Console.WriteLine($"Found {_ownerChannel.Count} dm Channel for {owners.Length} application owner");
        }

        private void SetupFeedback()
        {
            //Button response with modal
            _modularDiscordBot.DiscordClient.ComponentInteractionCreated += async (sender, e) =>
            {
                if (e.Id != "feedback-button") return;

                var modal = new DiscordInteractionResponseBuilder();

                modal
                .WithTitle("Feedback")
                .WithCustomId("feedback-modal")
                .AddComponents(new TextInputComponent(label: "Please enter your feedback:", customId: "feedback-text", required: true, style: TextInputStyle.Paragraph));

                await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
            };

            //Modal processing
            _modularDiscordBot.DiscordClient.ModalSubmitted += async (sender, e) =>
            {
                if (e.Interaction.Data.CustomId != "feedback-modal") return;

                DiscordInteractionResponseBuilder responseBuilder = new();
                DiscordEmbedBuilder embedBuilder = new();

                embedBuilder
                .WithTitle("Thank you for submitting your feedback")
                .WithColor(DiscordColor.Green);

                responseBuilder
                .AddEmbed(embedBuilder)
                .AsEphemeral();

                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder);

                var guildName = "Dms";
                if (e.Interaction.Guild is not null) { guildName = e.Interaction.Guild.Name; }

                var discordEmbed = new DiscordEmbedBuilder
                {
                    Title = "Feedback",
                    Description = e.Values["feedback-text"],
                    Color = new(new(0, 255, 194)),
                    Timestamp = DateTime.Now,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = "Send by "
                        + e.Interaction.User.Username
                        + " from "
                        + guildName
                    }
                };

                foreach (DiscordDmChannel channel in _ownerChannel)
                {
                    await channel.SendMessageAsync(discordEmbed);
                }
            };
        }

        public Task LogEvent(string message, string sender, LogLevel lvl)
        {
            string log = $"[{DateTime.Now:yyyy'-'MM'-'dd'T'HH':'mm':'ss}] [{lvl}] [{sender}] {message}";
            File.AppendAllTextAsync(_logPath, log + "\n", System.Text.Encoding.UTF8);
            return Task.CompletedTask;
        }

        public async Task LogCommandExecutionAsync(CommandContext ctx, DiscordMessage response)
        {
            var triggerTime = ctx.Message.Timestamp.DateTime;
            var executionTime = response.Timestamp.DateTime;
            var timespan = (executionTime - triggerTime).TotalMilliseconds;
            if (ctx.Command != null)
            {
                var commandName = ctx.Command.Name;
                var commandArgs = ctx.RawArguments;

                string tmp = commandArgs.Aggregate("", (current, arg) => current + (arg.ToString() + ", "));

                tmp = tmp.Remove(tmp.Length - 2);

                var logEntry = $"[{DateTime.Now:dd'.'MM'.'yyyy'-'HH':'mm':'ss}] [INFO] [{ctx.User.Username}#{ctx.User.Discriminator} : {ctx.User.Id}] [{commandName}] [{tmp}] {timespan} milliseconds to execute";
                await File.AppendAllTextAsync(_logPath, logEntry + "\n", System.Text.Encoding.UTF8);
            }
        }

        public async Task<List<DiscordMessage>> LogToOwner(string message, string sender, LogLevel logLevel)
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

            List<DiscordMessage> messageList = new();

            foreach (DiscordDmChannel channel in _ownerChannel)
            {
                messageList.Add(await channel.SendMessageAsync(discordEmbed));
            }

            return messageList;
        }
    }
}