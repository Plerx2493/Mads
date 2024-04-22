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

using DeepL;
using DeepL.Model;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MADS.CustomComponents;
using MADS.Extensions;
using MADS.Services;
using Quartz.Util;

namespace MADS.Commands.ContextMenu;

public class TranslateMessage : MadsBaseApplicationCommand
{
    private readonly TranslateInformationService _translateInformationService;
    private readonly Translator _translator;

    public TranslateMessage(TranslateInformationService translateInformationService, Translator translator)
    {
        _translateInformationService = translateInformationService;
        _translator = translator;
    }
    
    [ContextMenu(DiscordApplicationCommandType.MessageContextMenu, "Translate message")]
    public async Task TranslateAsync(ContextMenuContext ctx)
    {
        await ctx.DeferAsync(true);

        string? preferredLanguage = await _translateInformationService.GetPreferredLanguage(ctx.User.Id);
        bool isPreferredLanguageSet = !preferredLanguage.IsNullOrWhiteSpace();
       
        if(!isPreferredLanguageSet)
        {
            preferredLanguage = "en-US";
        }

        ulong messageId = ctx.TargetMessage.Id;
        DiscordMessage message = await ctx.Channel.GetMessageAsync(messageId);
        string? messageContent = message.Content;

        if (messageContent.IsNullOrWhiteSpace() || messageContent is null)
        {
            await ctx.CreateResponseAsync("⚠️ Message is empty!");
            return;
        }
        
        if (preferredLanguage is null)
        {
            await ctx.CreateResponseAsync("⚠️ No language set!");
            return;
        }
        
        TextResult translatedMessage = 
            await _translator.TranslateTextAsync(messageContent, null, preferredLanguage);
        
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithAuthor(message.Author?.Username, 
                message.Author?.AvatarUrl)
            .WithDescription(translatedMessage.Text)
            .WithColor(new DiscordColor(0, 255, 194))
            .WithFooter($"Translated from {translatedMessage.DetectedSourceLanguageCode} to {preferredLanguage}")
            .WithTimestamp(DateTime.Now);

        await ctx.CreateResponseAsync(embed);

        if (isPreferredLanguageSet)
        {
            return;
        }

        DiscordFollowupMessageBuilder followUpMessage = new DiscordFollowupMessageBuilder()
            .WithContent("⚠️ You haven't set a preferred language yet. Default is english.")
            .AddComponents(new DiscordButtonComponent(DiscordButtonStyle.Primary, "setLanguage", "Set language").AsActionButton(ActionDiscordButtonEnum.SetTranslationLanguage))
            .AddComponents(new DiscordButtonComponent(DiscordButtonStyle.Primary, "setLanguage", "Set your language to en-US").AsActionButton(ActionDiscordButtonEnum.SetTranslationLanguage, "en-US"))
            .AsEphemeral();
        
        
        await ctx.FollowUpAsync(followUpMessage);
    }
}