// Copyright 2023 Plerx2493
//
// Licensed under the Apache License, Version 2.0 (the "License")
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using MADS.Extensions;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MADS.Services;

public class LoggingService
{
    private static readonly Regex PrettyNameRegex = new("PRETTY_NAME=(.*)", RegexOptions.Compiled);

    //Utilities
    private readonly string _dirPath = DataProvider.GetPath("Logs");
    private readonly string _logPath;
    private readonly DiscordClientService _modularDiscordBot;
    private DiscordRestClient _discordRestClient;
    private DiscordWebhookClient _discordWebhookClient = new();
    private bool _isSetup;
    private List<DiscordDmChannel> _ownerChannel = new();
    
    private static Serilog.ILogger _logger = Log.ForContext<LoggingService>();

    internal LoggingService(DiscordClientService dBot)
    {
        Log.Warning("LoggingService");
        var startDate = DateTime.Now;
        _modularDiscordBot = dBot;
        Directory.CreateDirectory(_dirPath);
        var osVersion = Environment.OSVersion.VersionString;
        _logPath = DataProvider.GetPath("Logs",
            $"{startDate.Day}-{startDate.Month}-{startDate.Year}_{startDate.Hour}-{startDate.Minute}-{startDate.Second}.log");

        var os = osVersion.StartsWith("Unix") ? FetchLinuxName() : Environment.OSVersion.VersionString;

        File.AppendAllText(_logPath, $".Net: {RuntimeInformation.FrameworkDescription}\n", Encoding.UTF8);
        File.AppendAllText(_logPath, $"Operating system: {os}\n", Encoding.UTF8);
        File.AppendAllText(_logPath, "========== LOG START ==========\n\n", Encoding.UTF8);
    }

    //Fetching Linux name by Naamloos. Can be found in Naamloos/Modcore
    private static string FetchLinuxName()
    {
        try
        {
            var result = File.ReadAllText("/etc/os-release");
            var match = PrettyNameRegex.Match(result);
            return !match.Success ? Environment.OSVersion.VersionString : match.Groups[1].Value.Replace("\"", "");
        }
        catch
        {
            return Environment.OSVersion.VersionString;
        }
    }

    public void Setup()
    {
        if (_isSetup) return;

        AddRestClient();
        AddOwnerChannels();
        SetupFeedback();
        SetupWebhookLogging();

        _isSetup = true;
    }

    private void AddRestClient()
    {
        if (_discordRestClient != null) return;

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
        if (_ownerChannel.Count == owners.Length) return;

        _ownerChannel = new List<DiscordDmChannel>();

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

        _logger.Information(
            "Found {OwnerChannel} dm Channel for {Owner} application owner",
            _ownerChannel.Count, owners.Length);
    }

    private void SetupFeedback()
    {
        //Button response with modal
        _modularDiscordBot.DiscordClient.ComponentInteractionCreated += async (_, e) =>
        {
            if (e.Id != "feedback-button") return;

            DiscordInteractionResponseBuilder modal = new();

            modal
                .WithTitle("Feedback")
                .WithCustomId("feedback-modal")
                .AddComponents(new TextInputComponent("Please enter your feedback:", "feedback-text", required: true,
                    style: TextInputStyle.Paragraph));

            await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
        };

        //Modal processing
        _modularDiscordBot.DiscordClient.ModalSubmitted += async (_, e) =>
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

            string guildName;
            
            if (e.Interaction.Guild is null)
            {
                guildName = "Dms";
            }
            else
            {
                guildName = e.Interaction.Guild.Name;
            }

            var discordEmbed = new DiscordEmbedBuilder
            {
                Title = "Feedback",
                Description = e.Values["feedback-text"],
                Color = new DiscordColor(0, 255, 194),
                Timestamp = (DateTimeOffset) DateTime.Now,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "Send by " + e.Interaction.User.Username + " from " + guildName
                }
            };

            await _discordWebhookClient.BroadcastMessageAsync(new DiscordWebhookBuilder().AddEmbed(discordEmbed));
        };
    }

    private void SetupWebhookLogging()
    {
        _discordWebhookClient = new DiscordWebhookClient();
        var config = DataProvider.GetConfig();
        var webhookUrl = new Uri(config.DiscordWebhook);
        _discordWebhookClient.AddWebhookAsync(webhookUrl).GetAwaiter().GetResult();
    }

    public async Task LogEvent(string message, string sender, LogLevel lvl)
    {
        var log = $"[{DateTime.Now:yyyy'-'MM'-'dd'T'HH':'mm':'ss}] [{lvl}] [{sender}] {message}";
        await File.AppendAllTextAsync(_logPath, log + "\n", Encoding.UTF8);
    }

    public async Task LogCommandExecutionAsync(CommandContext ctx, TimeSpan timespan)
    {
        await LogInfo(
            $"[{ctx.User.Username}#{ctx.User.Discriminator} : {ctx.User.Id}] [{ctx.Command.Name}] {timespan.TotalMilliseconds} milliseconds to execute");
    }

    public async Task LogCommandExecutionAsync(InteractionContext ctx, TimeSpan timespan)
    {
        await LogInfo(
            $"[{ctx.User.Username}#{ctx.User.Discriminator} : {ctx.User.Id}] [/{ctx.CommandName}] {timespan.TotalMilliseconds} milliseconds to execute");
    }

    public async Task LogCommandExecutionAsync(ContextMenuContext ctx, TimeSpan timespan)
    {
        await LogInfo(
            $"[{ctx.User.Username}#{ctx.User.Discriminator} : {ctx.User.Id}] [CM-{ctx.CommandName}] {timespan.TotalMilliseconds} milliseconds to execute");
    }

    private async Task LogInfo(string input)
    {
        var logEntry =
            $"[{DateTime.Now:dd'.'MM'.'yyyy'-'HH':'mm':'ss}] [INFO]" + input;
        await File.AppendAllTextAsync(_logPath, logEntry + "\n", Encoding.UTF8);
    }

    public async Task<List<DiscordMessage>> LogToOwner(string message, string sender, LogLevel logLevel)
    {
        var discordEmbed = new DiscordEmbedBuilder
        {
            Title = logLevel.ToString(),
            Description = message,
            Color = new DiscordColor(0, 255, 194),
            Timestamp = DateTime.Now,
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = "Send by " + sender
            }
        };

        List<DiscordMessage> messageList = new();

        foreach (var channel in _ownerChannel) messageList.Add(await channel.SendMessageAsync(discordEmbed));

        return messageList;
    }

    public async Task LogToWebhook(DiscordMessageBuilder message)
    {
        var messageBuilder = new DiscordWebhookBuilder(message);

        await _discordWebhookClient.BroadcastMessageAsync(messageBuilder);
    }
}