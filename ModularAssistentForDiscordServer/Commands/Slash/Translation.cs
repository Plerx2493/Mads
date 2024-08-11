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

using System.ComponentModel;
using DeepL;
using DeepL.Model;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using MADS.Commands.AutoCompletion;
using MADS.Extensions;
using MADS.Services;

namespace MADS.Commands.Slash;

[Command("translation"), Description("Commands for translation")]
public class Translation
{
    private readonly TranslateInformationService _translationUserInfo;
    private readonly Translator _translator;

    public Translation(TranslateInformationService translationUserInfo, Translator translator)
    {
        _translationUserInfo = translationUserInfo;
        _translator = translator;
    }

    [Command("info"), Description("Get your preferred language")]
    public async Task InfoAsync(CommandContext ctx)
    {
        string? lang = await _translationUserInfo.GetPreferredLanguageAsync(ctx.User.Id);

        await ctx.RespondSuccessAsync($"Language set to `{lang ?? "null"}`");
    }

    [Command("setLanguage"), Description("Set your preferred language")]
    public async Task SetLanguageAsync
    (
        CommandContext ctx,
        [Description("The language you want to get (default: en)"), SlashAutoCompleteProvider(typeof(TargetLanguageAutoCompletion))]
        string language = "en"
    )
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            await ctx.RespondErrorAsync("⚠️ Language can't be empty!");
            return;
        }

        string code = TranslateInformationService.StandardizeLang(language);

        _translationUserInfo.SetPreferredLanguage(ctx.User.Id, code);

        await ctx.RespondSuccessAsync($"✅ Language set to {code}");
    }

    [Command("translate"), Description("Translate a text")]
    public async Task TranslateText
    (
        CommandContext ctx,
        [Description("The text you want to translate")]
        string text,
        [Description("The language you want to get (default: en)"), SlashAutoCompleteProvider(typeof(TargetLanguageAutoCompletion))]
        string? language = null,
        [Description("Weather the result should be public or not (default: false)")]
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
        
        if (language is null)
        {
            string? lang = await _translationUserInfo.GetPreferredLanguageAsync(ctx.User.Id);

            language = lang ?? "en-US";
        }

        TextResult translatedText = await _translator.TranslateTextAsync(text, null, language);

        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithDescription(translatedText.Text)
            .WithColor(new DiscordColor(0, 255, 194))
            .WithFooter($"Translated from {translatedText.DetectedSourceLanguageCode} to {language}")
            .WithTimestamp(DateTime.Now);

        DiscordInteractionResponseBuilder responseBuilder = new();
        responseBuilder.AddEmbed(embed).AsEphemeral(!publicResult);

        await ctx.RespondAsync(responseBuilder);
    }
}