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
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using MADS.Entities;
using MADS.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MADS.Services;

public class LoggingService
{
    private static readonly Regex PrettyNameRegex = new("PRETTY_NAME=(.*)", RegexOptions.Compiled);
    private readonly string _dirPath = DataProvider.GetPath("Logs");
    private readonly string _logPath;
    private DiscordRestClient _discordRestClient;
    private DiscordCommandService? _modularDiscordBot;
    private DiscordWebhookClient _discordWebhookClient = new();
    private bool _isSetup;
    private List<DiscordDmChannel> _ownerChannel = [];
    
    private static readonly Serilog.ILogger _logger = Log.ForContext<LoggingService>();
    
    public LoggingService()
    {
        Directory.CreateDirectory(_dirPath);
        
        DateTime startDate = DateTime.Now;
        _logPath = DataProvider.GetPath("Logs",
            $"{startDate.Day}-{startDate.Month}-{startDate.Year}_{startDate.Hour}-{startDate.Minute}-{startDate.Second}.log");
        
        string osVersion = Environment.OSVersion.VersionString;
        string os = osVersion.StartsWith("Unix") ? FetchLinuxName() : Environment.OSVersion.VersionString;
        
        File.AppendAllText(_logPath, $".Net: {RuntimeInformation.FrameworkDescription}\n", Encoding.UTF8);
        File.AppendAllText(_logPath, $"Operating system: {os}\n", Encoding.UTF8);
        File.AppendAllText(_logPath, "========== LOG START ==========\n\n", Encoding.UTF8);
    }
    
    //Fetching Linux name by Naamloos. Can be found in Naamloos/Modcore
    private static string FetchLinuxName()
    {
        try
        {
            string result = File.ReadAllText("/etc/os-release");
            Match match = PrettyNameRegex.Match(result);
            return !match.Success ? Environment.OSVersion.VersionString : match.Groups[1].Value.Replace("\"", "");
        }
        catch
        {
            return Environment.OSVersion.VersionString;
        }
    }
    
    public void Setup(DiscordCommandService modularDiscordBot)
    {
        _modularDiscordBot = modularDiscordBot;
        if (_isSetup)
        {
            return;
        }
        
        _discordRestClient = ModularDiscordBot.Services.GetRequiredService<DiscordRestClient>();
        AddOwnerChannels();
        SetupFeedback();
        SetupWebhookLogging();
        
        _isSetup = true;
    }
    
    private async void AddOwnerChannels()
    {
        DiscordApplication? application = _modularDiscordBot?.DiscordClient.CurrentApplication;
        DiscordUser[]? owners = application?.Owners?.ToArray();
        if (owners is null || owners.Length == 0)
        {
            return;
        }
        
        _ownerChannel = new List<DiscordDmChannel>();
        
        foreach (DiscordUser owner in owners)
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
        if (_modularDiscordBot is null)
        {
            throw new Exception("LoggingService is not set up.");
        }
        
        //Button response with modal
        _modularDiscordBot.DiscordClient.ComponentInteractionCreated += async (_, e) =>
        {
            if (e.Id != "feedback-button")
            {
                return;
            }
            
            DiscordInteractionResponseBuilder modal = new();
            
            modal
                .WithTitle("Feedback")
                .WithCustomId("feedback-modal")
                .AddComponents(new DiscordTextInputComponent("Please enter your feedback:", "feedback-text",
                    required: true,
                    style: DiscordTextInputStyle.Paragraph));
            
            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, modal);
        };
        
        //Modal processing
        _modularDiscordBot.DiscordClient.ModalSubmitted += async (_, e) =>
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
            
            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                responseBuilder);
            
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            string guildName =
                e.Interaction.Guild is null ? "Dms" : e.Interaction.Guild.Name;
            
            DiscordEmbedBuilder discordEmbed = new()
            {
                Title = "Feedback",
                Description = e.Values["feedback-text"],
                Color = new DiscordColor(0, 255, 194),
                Timestamp = (DateTimeOffset)DateTime.Now,
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
        MadsConfig config = DataProvider.GetConfig();
        Uri webhookUrl = new(config.DiscordWebhook);
        _discordWebhookClient.AddWebhookAsync(webhookUrl).GetAwaiter().GetResult();
    }
    
    public async Task<List<DiscordMessage>> LogToOwner(string message, string sender, LogLevel logLevel)
    {
        DiscordEmbedBuilder discordEmbed = new()
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
        
        List<DiscordMessage> messageList = [];
        
        foreach (DiscordDmChannel channel in _ownerChannel)
        {
            messageList.Add(await channel.SendMessageAsync(discordEmbed));
        }
        
        return messageList;
    }
    
    public async Task LogToWebhook(DiscordMessageBuilder message)
    {
        DiscordWebhookBuilder messageBuilder = new(message);
        
        await _discordWebhookClient.BroadcastMessageAsync(messageBuilder);
    }
}