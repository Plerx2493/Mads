﻿using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MADS.CustomComponents;
using MADS.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MADS.EventListeners;

internal static partial class EventListener
{
    internal static async Task DmHandler(DiscordClient client, MessageCreateEventArgs e)
    {
        if (!e.Channel.IsPrivate) return;
        if (e.Author.IsBot) return;

        //if (client.CurrentApplication.Owners.Contains(e.Author)) return Task.CompletedTask;
        
        //retrieves the config.json
        var config = DataProvider.GetConfig();

        //Create a discordWebhookClient and add the debug webhook from the config.json
        var webhookClient = new DiscordWebhookClient();
        var webhookUrl = new Uri(config.DiscordWebhook);
        webhookClient.AddWebhookAsync(webhookUrl).GetAwaiter().GetResult();
        
        
        var embed = new DiscordEmbedBuilder()
                             .WithAuthor("Mads-DMs")
                             .WithColor(new DiscordColor(0, 255, 194))
                             .WithTimestamp(DateTime.UtcNow)
                             .WithTitle($"Dm from {e.Author.Username}#{e.Author.Discriminator}")
                             .WithDescription(e.Message.Content);
        
        var button = new DiscordButtonComponent(ButtonStyle.Success, "Placeholder", "Respond to User").AsActionButton(ActionDiscordButtonEnum.AnswerDmChannel, e.Channel.Id);
        
        var channel = await client.GetChannelAsync(webhookClient.Webhooks[0].ChannelId);
        await channel.SendMessageAsync(new DiscordMessageBuilder().AddEmbed(embed).AddComponents(button));
    }
}