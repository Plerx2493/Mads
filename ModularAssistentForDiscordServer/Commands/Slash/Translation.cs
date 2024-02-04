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
using MADS.Extensions;
using MADS.Services;

namespace MADS.Commands.Slash;

[SlashCommandGroup("translation", "Commands for translation")]
public class Translation : MadsBaseApplicationCommand
{
    private readonly TranslateInformationService _translationUserInfo;
    private readonly Translator _translator;
    
    public Translation(TranslateInformationService translationUserInfo, Translator translator)
    {
        _translationUserInfo = translationUserInfo;
        _translator = translator;
    }
    
    [SlashCommand("setLanguage", "Set your prefered language")]
    public async Task SetLanguageAsync
    (
        InteractionContext ctx,
        [Option("language", "The language you want to set")] 
        string language
    )
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder()
                    .WithContent("⚠️ Language can't be empty!"));
            return;
        }

        var code = TranslateInformationService.StandardizeLang(language);
        
        _translationUserInfo.SetPreferredLanguage(ctx.User.Id, code);
        
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder()
                .WithContent($"✅ Language set to {code}"));
    }

    [SlashCommand("translate", "Translate a text")]
    public async Task TranslateText
    (
        InteractionContext ctx,
        [Option("text", "The text you want to translate")]
        string text,
        [Option("language", "The language you want to get (default: en)")]
        string language = "en",
        [Option("publicResult", "Weather the result should be public or not (default: false)")]
        bool publicResult = false
    )
    {
        await ctx.DeferAsync();
        
        if (string.IsNullOrWhiteSpace(text))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent("⚠️ Text can't be empty!"));
            return;
        }
        
        var code = TranslateInformationService.StandardizeLang(language);
        
        var translatedText = await _translator.TranslateTextAsync(text, null, code);
        
        var embed = new DiscordEmbedBuilder()
            .WithDescription(translatedText.Text)
            .WithColor(new DiscordColor(0, 255, 194))
            .WithFooter($"Translated from {translatedText.DetectedSourceLanguageCode} to {code}")
            .WithTimestamp(DateTime.Now);

        await ctx.CreateResponseAsync(embed, publicResult);
    }
}