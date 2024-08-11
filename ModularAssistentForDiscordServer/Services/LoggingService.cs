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
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using MADS.Entities;
using MADS.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MADS.Services;

public class LoggingService : IEventHandler<ComponentInteractionCreatedEventArgs>, IEventHandler<ModalSubmittedEventArgs>
{
    private DiscordWebhookClient _discordWebhookClient = new();
    
    public LoggingService(DiscordCommandService modularDiscordBot)
    {
        SetupWebhookLogging();
    }

    private async Task HandleFeedbackButton(DiscordClient _, ComponentInteractionCreatedEventArgs e)
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
    }

    private async Task HandleFeedbackModal(DiscordClient _, ModalSubmittedEventArgs e)
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
    }

    private void SetupWebhookLogging()
    {
        _discordWebhookClient = new DiscordWebhookClient();
        MadsConfig config = DataProvider.GetConfig();
        Uri webhookUrl = new(config.DiscordWebhook);
        _discordWebhookClient.AddWebhookAsync(webhookUrl).GetAwaiter().GetResult();
    }
    
    public async Task LogToWebhook(DiscordMessageBuilder message)
    {
        DiscordWebhookBuilder messageBuilder = new(message);
        
        await _discordWebhookClient.BroadcastMessageAsync(messageBuilder);
    }

    public Task HandleEventAsync(DiscordClient sender, ComponentInteractionCreatedEventArgs eventArgs) => HandleFeedbackButton(sender, eventArgs);

    public Task HandleEventAsync(DiscordClient sender, ModalSubmittedEventArgs eventArgs) => HandleFeedbackModal(sender, eventArgs);
}