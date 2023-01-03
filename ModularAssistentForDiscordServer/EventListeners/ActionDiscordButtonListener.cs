﻿using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using MADS.CustomComponents;
using Microsoft.Extensions.Logging;

namespace MADS.EventListeners;

internal static partial class EventListener
{
    public static void EnableButtonListener(DiscordClient client)
    {
        client.ComponentInteractionCreated += Task(sender, e) =>
        {
            try
            {
                sender.Logger.LogTrace(e?.Id);

                if (!Regex.IsMatch(e.Id, @"^CMD:\d{1,4}(?::\d{1,20}){0,3}$"))
                {
                    return Task.CompletedTask;
                }

                var substring = e.Id.Split(':');
                if (!int.TryParse(substring[1], out var actionCode))
                {
                    return Task.CompletedTask;
                }

                substring = substring.Skip(1).ToArray();

                switch (actionCode)
                {
                    case (int)ActionDiscordButtonEnum.BanUser:
                        BanUser(e, substring);
                        break;

                    case (int)ActionDiscordButtonEnum.KickUser:
                        KickUser(e, substring);
                        break;

                    case (int)ActionDiscordButtonEnum.GetIdUser:
                        GetUserId(e, substring);
                        break;

                    case (int)ActionDiscordButtonEnum.GetIdGuild:
                        GetGuildId(e, substring);
                        break;

                    case (int)ActionDiscordButtonEnum.GetIdChannel:
                        GetChannelId(e, substring);
                        break;

                    case (int)ActionDiscordButtonEnum.MoveVoiceChannel:
                        MoveVoiceChannelUser(e, substring);
                        break;
                    
                    case (int)ActionDiscordButtonEnum.DeleteOneUserOnly: 
                        DeleteOneUserOnly(e, substring);
                        break;
                }
            }
            catch (Exception exception)
            {
                var _ = MainProgram.LogToWebhookAsync(exception);
            }
            return Task.CompletedTask;
        };
    }

    private static async void DeleteOneUserOnly(ComponentInteractionCreateEventArgs e, IReadOnlyList<string> substring)
    {
        if (e.User.Id.ToString() != substring[1]) return;
        await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
        await e.Message.DeleteAsync();
    }

    private static async void MoveVoiceChannelUser(ComponentInteractionCreateEventArgs e,
        IReadOnlyList<string> substring)
    {

        var member = await e.Guild.GetMemberAsync(e.User.Id);
        if (!member.Permissions.HasPermission(Permissions.MoveMembers)) { return; }
        var originChannel = e.Guild.GetChannel(ulong.Parse(substring[1]));
        var targetChannel = e.Guild.GetChannel(ulong.Parse(substring[2]));

        foreach (var voiceMember in originChannel.Users)
        {
            await targetChannel.PlaceMemberAsync(voiceMember);
        }

        await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
    }

    private static async void BanUser(ComponentInteractionCreateEventArgs e, IReadOnlyList<string> substring)
    {
        var member = await e.Guild.GetMemberAsync(e.User.Id);
        if (!member.Permissions.HasPermission(Permissions.BanMembers)) { return; }

        var userId = ulong.Parse(substring[1]);
        await e.Guild.BanMemberAsync(userId);
        await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
    }

    private static async void KickUser(ComponentInteractionCreateEventArgs e, IReadOnlyList<string> substring)
    {
        var member = await e.Guild.GetMemberAsync(e.User.Id);
        if (!member.Permissions.HasPermission(Permissions.KickMembers)) { return; }

        var userId = ulong.Parse(substring[1]);
        await e.Guild.BanMemberAsync(userId);
        await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
    }

    private static async void GetUserId(InteractionCreateEventArgs e, IReadOnlyList<string> substring)
    {
        var response = new DiscordInteractionResponseBuilder();

        response.WithContent("User id: " + ulong.Parse(substring[1]))
                .AsEphemeral();

        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
    }

    private static async void GetGuildId(InteractionCreateEventArgs e, IReadOnlyList<string> substring)
    {
        var response = new DiscordInteractionResponseBuilder();

        response.WithContent("Guild id: " + ulong.Parse(substring[1]))
                .AsEphemeral();

        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
    }

    private static async void GetChannelId(InteractionCreateEventArgs e, IReadOnlyList<string> substring)
    {
        var response = new DiscordInteractionResponseBuilder();

        response.WithContent("Channel id: " + ulong.Parse(substring[1]))
                .AsEphemeral();

        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
    }
}