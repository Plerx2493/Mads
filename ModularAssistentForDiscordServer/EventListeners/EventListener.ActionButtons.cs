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
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using MADS.CustomComponents;
using MADS.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MADS.EventListeners;

internal static partial class EventListener
{
    public static async Task ActionButtons(DiscordClient sender, ComponentInteractionCreatedEventArgs e)
    {
        //Format of the button id: CMD:ActionCode:arg1:arg2:arg3
        try
        {
            sender.Logger.LogTrace("Button clicked with id: {Id}", e.Id);
            
            if (!CommandButtonRegex().IsMatch(e.Id))
            {
                return;
            }
            
            string[] substring = e.Id.Split(':');
            if (!int.TryParse(substring[1], out int actionCode))
            {
                return;
            }
            
            substring = substring.Skip(1).ToArray();
            
            switch (actionCode)
            {
                case (int)ActionDiscordButtonEnum.BanUser:
                    await BanUser(e, substring);
                    break;
                
                case (int)ActionDiscordButtonEnum.KickUser:
                    await KickUser(e, substring);
                    break;
                
                case (int)ActionDiscordButtonEnum.GetIdUser:
                    await GetUserId(e, substring);
                    break;
                
                case (int)ActionDiscordButtonEnum.GetIdGuild:
                    await GetGuildId(e, substring);
                    break;
                
                case (int)ActionDiscordButtonEnum.GetIdChannel:
                    await GetChannelId(e, substring);
                    break;
                
                case (int)ActionDiscordButtonEnum.MoveVoiceChannel:
                    await MoveVoiceChannelUser(e, substring);
                    break;
                
                case (int)ActionDiscordButtonEnum.DeleteOneUserOnly:
                    await DeleteOneUserOnly(e, substring);
                    break;
                
                case (int)ActionDiscordButtonEnum.AnswerDmChannel:
                    await AnswerDmChannel(e, sender, substring);
                    break;
                
                case (int)ActionDiscordButtonEnum.SetTranslationLanguage:
                    await SetTranslationLanguage(e, sender, substring);
                    break;
            }
        }
        catch (Exception exception)
        {
            await MainProgram.LogToWebhookAsync(exception);
        }
    }
    
    // format: 8:(languageCode)?
    private static async Task SetTranslationLanguage(ComponentInteractionCreatedEventArgs args, DiscordClient sender,
        string[] substring)
    {
        TranslateInformationService translationService =
            ModularDiscordBot.Services.GetRequiredService<TranslateInformationService>();
        
        DiscordEmbedBuilder langSetEmbed = new DiscordEmbedBuilder()
            .WithTitle("Language set successful!")
            .WithColor(DiscordColor.Green);
        
        if (substring.Length == 1)
        {
            DiscordInteractionResponseBuilder modal = new DiscordInteractionResponseBuilder()
                .WithTitle("Set your preferred language:")
                .WithCustomId($"setLanguage-{args.User.Id}")
                .AddComponents(new DiscordTextInputComponent("Please enter your preferred language:", "answer-text",
                    required: true,
                    style: DiscordTextInputStyle.Paragraph));
            await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, modal);
            
            InteractivityExtension interactive = sender.GetInteractivity();
            InteractivityResult<ModalSubmittedEventArgs> result =
                await interactive.WaitForModalAsync($"setLanguage-{args.User.Id}");
            
            if (result.TimedOut)
            {
                await args.Interaction.CreateFollowupMessageAsync(
                    new DiscordFollowupMessageBuilder().WithContent("408 - Request Timeout"));
                return;
            }
            
            translationService.SetPreferredLanguage(args.User.Id, result.Result.Values["answer-text"]);
            return;
        }
        
        if (substring.Length != 2)
        {
            await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("400 - Bad Request").AsEphemeral());
            return;
        }
        
        translationService.SetPreferredLanguage(args.User.Id, substring[2]);
        
        await args.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AddEmbed(langSetEmbed).AsEphemeral());
    }
    
    private static async Task AnswerDmChannel(ComponentInteractionCreatedEventArgs e, DiscordClient client,
        IReadOnlyList<string> substring)
    {
        if (!(client.CurrentApplication.Owners?.Contains(e.User) ?? false))
        {
            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("401 - Unauthorized").AsEphemeral());
            return;
        }
        
        DiscordInteractionResponseBuilder modal = new DiscordInteractionResponseBuilder()
            .WithTitle("Answer to user:")
            .WithCustomId($"AnswerDM-{substring[1]}")
            .AddComponents(new DiscordTextInputComponent("Please enter your answer:", "answer-text", required: true,
                style: DiscordTextInputStyle.Paragraph));
        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, modal);
        
        
        InteractivityExtension interactive = client.GetInteractivity();
        InteractivityResult<ModalSubmittedEventArgs> result =
            await interactive.WaitForModalAsync($"AnswerDM-{substring[1]}");
        
        if (result.TimedOut)
        {
            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("408 - Request Timeout").AsEphemeral());
            return;
        }
        
        await result.Result.Interaction.DeferAsync(true);
        
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithDescription(result.Result.Values["answer-text"])
            .WithColor(DiscordColor.Green)
            .WithAuthor(result.Result.Interaction.User.Username + "#" + result.Result.Interaction.User.Discriminator)
            .WithFooter("Answer form a developer");
        
        try
        {
            DiscordChannel channel = await client.GetChannelAsync(ulong.Parse(substring[1]));
            await channel.SendMessageAsync(embed);
        }
        catch (Exception exception)
        {
            await result.Result.Interaction.EditOriginalResponseAsync(
                new DiscordWebhookBuilder().WithContent($"500 - Internal Server Error ({exception.GetType()})"));
            return;
        }
        
        DiscordEmbedBuilder resultEmbed = new DiscordEmbedBuilder()
            .WithTitle("Answer successful!")
            .WithColor(DiscordColor.Green);
        await result.Result.Interaction.EditOriginalResponseAsync(new DiscordWebhookBuilder().AddEmbed(resultEmbed));
        
        DiscordMessageBuilder editedMessage = new(e.Message);
        editedMessage.ClearComponents();
        editedMessage.AddComponents(
            new DiscordButtonComponent(DiscordButtonStyle.Success, "invalid", "Already answered", true));
        
        
        await e.Message.ModifyAsync(editedMessage);
    }
    
    private static async Task DeleteOneUserOnly(ComponentInteractionCreatedEventArgs e, IReadOnlyList<string> substring)
    {
        if (e.User.Id.ToString() != substring[1])
        {
            return;
        }
        
        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
        await e.Message.DeleteAsync();
    }
    
    private static async Task MoveVoiceChannelUser
    (
        ComponentInteractionCreatedEventArgs e,
        IReadOnlyList<string> substring
    )
    {
        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
        
        DiscordMember member = await e.Guild.GetMemberAsync(e.User.Id);
        if (!member.Permissions.HasPermission(DiscordPermissions.MoveMembers))
        {
            return;
        }
        
        DiscordChannel originChannel = await e.Guild.GetChannelAsync(ulong.Parse(substring[1]));
        DiscordChannel targetChannel = await e.Guild.GetChannelAsync(ulong.Parse(substring[2]));
        
        foreach (DiscordMember voiceMember in originChannel.Users)
        {
            await targetChannel.PlaceMemberAsync(voiceMember);
        }
    }
    
    private static async Task BanUser(ComponentInteractionCreatedEventArgs e, IReadOnlyList<string> substring)
    {
        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
        DiscordMember member = await e.Guild.GetMemberAsync(e.User.Id);
        if (!member.Permissions.HasPermission(DiscordPermissions.BanMembers))
        {
            return;
        }
        
        ulong userId = ulong.Parse(substring[1]);
        await e.Guild.BanMemberAsync(userId);
    }
    
    private static async Task KickUser(ComponentInteractionCreatedEventArgs e, IReadOnlyList<string> substring)
    {
        DiscordMember member = await e.Guild.GetMemberAsync(e.User.Id);
        if (!member.Permissions.HasPermission(DiscordPermissions.KickMembers))
        {
            return;
        }
        
        ulong userId = ulong.Parse(substring[1]);
        await e.Guild.BanMemberAsync(userId);
        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.DeferredMessageUpdate);
    }
    
    private static async Task GetUserId(InteractionCreatedEventArgs e, IReadOnlyList<string> substring)
    {
        DiscordInteractionResponseBuilder response = new();
        
        response.WithContent("User id: " + ulong.Parse(substring[1]))
            .AsEphemeral();
        
        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, response);
    }
    
    private static async Task GetGuildId(InteractionCreatedEventArgs e, IReadOnlyList<string> substring)
    {
        DiscordInteractionResponseBuilder response = new();
        
        response.WithContent("Guild id: " + ulong.Parse(substring[1]))
            .AsEphemeral();
        
        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, response);
    }
    
    private static async Task GetChannelId(InteractionCreatedEventArgs e, IReadOnlyList<string> substring)
    {
        DiscordInteractionResponseBuilder response = new();
        
        response.WithContent("Channel id: " + ulong.Parse(substring[1]))
            .AsEphemeral();
        
        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, response);
    }
    
    [GeneratedRegex(@"^CMD:\d{1,4}(?::\d{1,20}){0,3}$", RegexOptions.Compiled)]
    private static partial Regex CommandButtonRegex();
}