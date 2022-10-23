﻿using System.Text;
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
    private readonly string                 _dirPath = DataProvider.GetPath("Logs");
    private readonly string                 _logPath;
    private readonly ModularDiscordBot      _modularDiscordBot;
    private readonly List<DiscordDmChannel> _ownerChannel = new();

    internal LoggingProvider(ModularDiscordBot dBot)
    {
        var startDate = DateTime.Now;
        _modularDiscordBot = dBot;
        Directory.CreateDirectory(_dirPath);

        _logPath = DataProvider.GetPath("Logs",
            $"{startDate.Day}-{startDate.Month}-{startDate.Year}_{startDate.Hour}-{startDate.Minute}-{startDate.Second}.log");
        File.AppendAllTextAsync(_logPath, "========== LOG START ==========\n\n", Encoding.UTF8);
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

        foreach (var owner in owners)
        {
            DiscordMember member = null;

            foreach (var guild in guilds)
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

            foreach (var channel in _ownerChannel)
            {
                await channel.SendMessageAsync(discordEmbed);
            }
        };
    }

    public Task LogEvent(string message, string sender, LogLevel lvl)
    {
        var log = $"[{DateTime.Now:yyyy'-'MM'-'dd'T'HH':'mm':'ss}] [{lvl}] [{sender}] {message}";
        File.AppendAllTextAsync(_logPath, log + "\n", Encoding.UTF8);
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

            var tmp = commandArgs.Aggregate("", (current, arg) => current + arg.ToString() + ", ");

            tmp = tmp.Remove(tmp.Length - 2);

            var logEntry =
                $"[{DateTime.Now:dd'.'MM'.'yyyy'-'HH':'mm':'ss}] [INFO] [{ctx.User.Username}#{ctx.User.Discriminator} : {ctx.User.Id}] [{commandName}] [{tmp}] {timespan} milliseconds to execute";
            await File.AppendAllTextAsync(_logPath, logEntry + "\n", Encoding.UTF8);
        }
    }

    public async Task LogCommandExecutionAsync(InteractionContext ctx, DiscordMessage response)
    {
        var triggerTime = ctx.Interaction.CreationTimestamp.DateTime;
        var executionTime = response.Timestamp.DateTime;
        var timespan = (executionTime - triggerTime).TotalMilliseconds;
        var commandName = ctx.CommandName;
   
        var logEntry =
            $"[{DateTime.Now:dd'.'MM'.'yyyy'-'HH':'mm':'ss}] [INFO] [{ctx.User.Username}#{ctx.User.Discriminator} : {ctx.User.Id}] [{commandName}] {timespan} milliseconds to execute";
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