using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;

namespace MADS.Extensions;

public class LoggingProvider
{
    //Utilities
    private readonly string                 _dirPath              = DataProvider.GetPath("Logs");
    private readonly DiscordWebhookClient   _discordWebhookClient = new();
    private readonly string                 _logPath;
    private readonly ModularDiscordBot      _modularDiscordBot;
    private readonly List<DiscordDmChannel> _ownerChannel = new();
    private          DiscordRestClient      _discordRestClient;

    internal LoggingProvider(ModularDiscordBot dBot)
    {
        var startDate = DateTime.Now;
        _modularDiscordBot = dBot;
        Directory.CreateDirectory(_dirPath);

        _logPath = DataProvider.GetPath("Logs",
            $"{startDate.Day}-{startDate.Month}-{startDate.Year}_{startDate.Hour}-{startDate.Minute}-{startDate.Second}.log");
        File.AppendAllTextAsync(_logPath, "========== LOG START ==========\n\n", Encoding.UTF8);
        
        Console.WriteLine(_logPath);
    }

    public async void Setup()
    {
        AddRestClient();
        AddOwnerChannels();
        SetupFeedback();
        SetupWebhookLogging();
    }

    private void AddRestClient()
    {
        var config = DataProvider.GetConfig();

        var discordConfig = new DiscordConfiguration
        {
            Token = config.Token
        };

        _discordRestClient = new DiscordRestClient(discordConfig);
    }

    private async void AddOwnerChannels()
    {
        var application = _modularDiscordBot.DiscordClient.CurrentApplication;
        var owners = application.Owners.ToArray();

        foreach (var owner in owners)
        {
            DiscordDmChannel ownerChannel;

            try
            {
                ownerChannel = await _discordRestClient.CreateDmAsync(owner.Id);
            }
            catch (DiscordException)
            {
                continue;
            }

            _ownerChannel.Add(ownerChannel);
        }

        _modularDiscordBot.DiscordClient.Logger.LogInformation("Found {1} dm Channel for {2} application owner",
            _ownerChannel.Count, owners.Length);
    }

    private void SetupFeedback()
    {
        //Button response with modal
        _modularDiscordBot.DiscordClient.ComponentInteractionCreated += async (sender, e) =>
        {
            if (e.Id != "feedback-button")
            {
                return;
            }

            DiscordInteractionResponseBuilder modal = new();

            modal
                .WithTitle("Feedback")
                .WithCustomId("feedback-modal")
                .AddComponents(new TextInputComponent("Please enter your feedback:", "feedback-text", required: true,
                    style: TextInputStyle.Paragraph));

            await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
        };

        //Modal processing
        _modularDiscordBot.DiscordClient.ModalSubmitted += async (sender, e) =>
        {
            if (e.Interaction.Data.CustomId != "feedback-modal")
            {
                return;
            }

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
                Color = new Optional<DiscordColor>(new DiscordColor(0, 255, 194)),
                Timestamp = DateTime.Now,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "Send by "
                           + e.Interaction.User.Username
                           + " from "
                           + guildName
                }
            };

            await _discordWebhookClient.BroadcastMessageAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbed));
        };
    }

    private void SetupWebhookLogging()
    {
        var config = DataProvider.GetConfig();
        var webhookUrl = new Uri(config.DiscordWebhook);
        _discordWebhookClient.AddWebhookAsync(webhookUrl).GetAwaiter().GetResult();
    }

    public Task LogEvent(string message, string sender, LogLevel lvl)
    {
        var log = $"[{DateTime.Now:yyyy'-'MM'-'dd'T'HH':'mm':'ss}] [{lvl}] [{sender}] {message}";
        File.AppendAllTextAsync(_logPath, log + "\n", Encoding.UTF8);
        return Task.CompletedTask;
    }

    public async Task LogCommandExecutionAsync(CommandContext ctx, TimeSpan timespan)
    {
        if (ctx.Command != null)
        {
            var commandName = ctx.Command.Name;
            var logEntry =
                $"[{DateTime.Now:dd'.'MM'.'yyyy'-'HH':'mm':'ss}] [INFO] [{ctx.User.Username}#{ctx.User.Discriminator} : {ctx.User.Id}] [{commandName}]{timespan.Microseconds} milliseconds to execute";
            await File.AppendAllTextAsync(_logPath, logEntry + "\n", Encoding.UTF8);
        }
    }

    public async Task LogCommandExecutionAsync(InteractionContext ctx, TimeSpan timespan)
    {
        var logEntry =
            $"[{DateTime.Now:dd'.'MM'.'yyyy'-'HH':'mm':'ss}] [INFO] [{ctx.User.Username}#{ctx.User.Discriminator} : {ctx.User.Id}] [/{ctx.CommandName}] {timespan.Microseconds} milliseconds to execute";
        await File.AppendAllTextAsync(_logPath, logEntry + "\n", Encoding.UTF8);
    }
    
    public async Task LogCommandExecutionAsync(ContextMenuContext ctx, TimeSpan timespan)
    {
        var logEntry =
            $"[{DateTime.Now:dd'.'MM'.'yyyy'-'HH':'mm':'ss}] [INFO] [{ctx.User.Username}#{ctx.User.Discriminator} : {ctx.User.Id}] [/{ctx.CommandName}] {timespan.Microseconds} milliseconds to execute";
        await File.AppendAllTextAsync(_logPath, logEntry + "\n", Encoding.UTF8);
    }

    public async Task<List<DiscordMessage>> LogToOwner(string message, string sender, LogLevel logLevel)
    {
        var discordEmbed = new DiscordEmbedBuilder
        {
            Title = logLevel.ToString(),
            Description = message,
            Color = new Optional<DiscordColor>(new DiscordColor(0, 255, 194)),
            Timestamp = DateTime.Now,
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = "Send by " + sender
            }
        };

        List<DiscordMessage> messageList = new();

        foreach (var channel in _ownerChannel)
        {
            messageList.Add(await channel.SendMessageAsync(discordEmbed));
        }

        return messageList;
    }
}