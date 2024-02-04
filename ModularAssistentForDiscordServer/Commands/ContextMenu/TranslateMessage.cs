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
using DSharpPlus;
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
    
    [ContextMenu(ApplicationCommandType.MessageContextMenu, "Translate message")]
    public async Task TranslateAsync(ContextMenuContext ctx)
    {
        await ctx.DeferAsync(true);

        var preferredLanguage = await _translateInformationService.GetPreferredLanguage(ctx.User.Id);
        bool isPreferredLanguageSet = !preferredLanguage.IsNullOrWhiteSpace();
       
        if(!isPreferredLanguageSet) preferredLanguage = "en-US";
        
        var messageId = ctx.TargetMessage.Id;
        var message = await ctx.Channel.GetMessageAsync(messageId);
        var messageContent = message.Content;

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
        
        var transaltedMessage = 
            await _translator.TranslateTextAsync(messageContent, null, preferredLanguage);
        
        var embed = new DiscordEmbedBuilder()
            .WithAuthor(message.Author?.Username, 
                message.Author?.AvatarUrl)
            .WithDescription(transaltedMessage.Text)
            .WithColor(new DiscordColor(0, 255, 194))
            .WithFooter($"Translated from {transaltedMessage.DetectedSourceLanguageCode} to {preferredLanguage}")
            .WithTimestamp(DateTime.Now);

        await ctx.CreateResponseAsync(embed);

        if (isPreferredLanguageSet) return;

        var followUpMessage = new DiscordFollowupMessageBuilder()
            .WithContent("⚠️ You haven't set a preferred language yet. Default is english.")
            .AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "setLanguage", "Set language").AsActionButton(ActionDiscordButtonEnum.SetTranslationLanguage))
            .AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "setLanguage", "Set your language to en-US").AsActionButton(ActionDiscordButtonEnum.SetTranslationLanguage, "en-US"))
            .AsEphemeral();
        
        
        await ctx.FollowUpAsync(followUpMessage);
    }
}