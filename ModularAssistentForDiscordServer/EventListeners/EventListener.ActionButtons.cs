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

using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using MADS.CustomComponents;
using Microsoft.Extensions.Logging;

namespace MADS.EventListeners;

internal static partial class EventListener
{
    public static void EnableButtonListener(DiscordClient client)
    {
        client.ComponentInteractionCreated += Task (sender, e) =>
        {
            try
            {
                sender.Logger.LogTrace("Button clicked with id: {Id}", e.Id);

                if (!CommandButtonRegex().IsMatch(e.Id))
                    return Task.CompletedTask;

                var substring = e.Id.Split(':');
                if (!int.TryParse(substring[1], out var actionCode)) return Task.CompletedTask;

                substring = substring.Skip(1).ToArray();

                switch (actionCode)
                {
                    case (int) ActionDiscordButtonEnum.BanUser:
                        BanUser(e, substring);
                        break;

                    case (int) ActionDiscordButtonEnum.KickUser:
                        KickUser(e, substring);
                        break;

                    case (int) ActionDiscordButtonEnum.GetIdUser:
                        GetUserId(e, substring);
                        break;

                    case (int) ActionDiscordButtonEnum.GetIdGuild:
                        GetGuildId(e, substring);
                        break;

                    case (int) ActionDiscordButtonEnum.GetIdChannel:
                        GetChannelId(e, substring);
                        break;

                    case (int) ActionDiscordButtonEnum.MoveVoiceChannel:
                        MoveVoiceChannelUser(e, substring);
                        break;

                    case (int) ActionDiscordButtonEnum.DeleteOneUserOnly:
                        DeleteOneUserOnly(e, substring);
                        break;
                    case (int) ActionDiscordButtonEnum.AnswerDmChannel:
                        AnswerDmChannel(e, sender, substring);
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

    private static async void AnswerDmChannel(ComponentInteractionCreateEventArgs e, DiscordClient client,
        IReadOnlyList<string> substring)
    {
        if (!client.CurrentApplication.Owners.Contains(e.User))
        {
            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("401 - Unauthorized").AsEphemeral());
            return;
        }

        var modal = new DiscordInteractionResponseBuilder()
            .WithTitle("Answer to user:")
            .WithCustomId($"AnswerDM-{substring[1]}")
            .AddComponents(new TextInputComponent("Please enter your answer:", "answer-text", required: true,
                style: TextInputStyle.Paragraph));
        await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);


        var interactive = client.GetInteractivity();
        var result = await interactive.WaitForModalAsync($"AnswerDM-{substring[1]}");

        if (result.TimedOut)
        {
            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("408 - Request Timeout").AsEphemeral());
            return;
        }

        await result.Result.Interaction.DeferAsync(true);

        var embed = new DiscordEmbedBuilder()
            .WithDescription(result.Result.Values["answer-text"])
            .WithColor(DiscordColor.Green)
            .WithAuthor(result.Result.Interaction.User.Username + "#" + result.Result.Interaction.User.Discriminator)
            .WithFooter("Answer form a developer");

        try
        {
            var channel = await client.GetChannelAsync(ulong.Parse(substring[1]));
            await channel.SendMessageAsync(embed);
        }
        catch (Exception exception)
        {
            await result.Result.Interaction.EditOriginalResponseAsync(
                new DiscordWebhookBuilder().WithContent($"500 - Internal Server Error ({exception.GetType()})"));
            return;
        }

        var resultEmbed = new DiscordEmbedBuilder()
            .WithTitle("Answer successful!")
            .WithColor(DiscordColor.Green);
        await result.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(resultEmbed));

        var editedMessage = new DiscordMessageBuilder(e.Message);
        editedMessage.ClearComponents();
        editedMessage.AddComponents(
            new DiscordButtonComponent(ButtonStyle.Success, "invalid", "Already answered", true));


        await e.Message.ModifyAsync(editedMessage);
    }

    private static async void DeleteOneUserOnly(ComponentInteractionCreateEventArgs e, IReadOnlyList<string> substring)
    {
        if (e.User.Id.ToString() != substring[1]) return;
        await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
        await e.Message.DeleteAsync();
    }

    private static async void MoveVoiceChannelUser
    (
        ComponentInteractionCreateEventArgs e,
        IReadOnlyList<string> substring
    )
    {
        var member = await e.Guild.GetMemberAsync(e.User.Id);
        if (!member.Permissions.HasPermission(Permissions.MoveMembers)) return;
        var originChannel = e.Guild.GetChannel(ulong.Parse(substring[1]));
        var targetChannel = e.Guild.GetChannel(ulong.Parse(substring[2]));

        foreach (var voiceMember in originChannel.Users) await targetChannel.PlaceMemberAsync(voiceMember);

        await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
    }

    private static async void BanUser(ComponentInteractionCreateEventArgs e, IReadOnlyList<string> substring)
    {
        var member = await e.Guild.GetMemberAsync(e.User.Id);
        if (!member.Permissions.HasPermission(Permissions.BanMembers)) return;

        var userId = ulong.Parse(substring[1]);
        await e.Guild.BanMemberAsync(userId);
        await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
    }

    private static async void KickUser(ComponentInteractionCreateEventArgs e, IReadOnlyList<string> substring)
    {
        var member = await e.Guild.GetMemberAsync(e.User.Id);
        if (!member.Permissions.HasPermission(Permissions.KickMembers)) return;

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

    [GeneratedRegex("^CMD:\\d{1,4}(?::\\d{1,20}){0,3}$", RegexOptions.Compiled)]
    private static partial Regex CommandButtonRegex();
}