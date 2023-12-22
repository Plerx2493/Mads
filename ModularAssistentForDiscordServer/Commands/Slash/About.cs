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

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Humanizer;
using MADS.Extensions;

namespace MADS.Commands.Slash;

public sealed class About : MadsBaseApplicationCommand
{
    [SlashCommand("about", "Infos about the bot")]
    public async Task AboutCommand(InteractionContext ctx)
    {
        var discordEmbedBuilder = CommandUtility.GetDiscordEmbed();
        var discordMessageBuilder = new DiscordInteractionResponseBuilder();
        var inviteUri = ctx.Client.CurrentApplication.GenerateOAuthUri(null, Permissions.Administrator, DiscordOAuthScope.Bot,
            DiscordOAuthScope.ApplicationsCommands);
        var addMe = $"[Click here!]({inviteUri.Replace(" ", "%20")})";

        var diff = DateTime.Now - CommandService.StartTime;
        var date = $"{diff.Days} days {diff.Hours} hours {diff.Minutes} minutes";

        discordEmbedBuilder
            .WithTitle("About me")
            .WithDescription("A modular designed discord bot for moderation and stuff")
            .WithAuthor(ctx.Client.CurrentUser.Username, ctx.Client.CurrentUser.AvatarUrl,
                ctx.Client.CurrentUser.AvatarUrl)
            .WithColor(new DiscordColor(0, 255, 194))
            .AddField("Owner:", "[Plerx#0175](https://github.com/Plerx2493/)", true)
            .AddField("Source:", "[Github](https://github.com/Plerx2493/Mads)", true)
            .AddField("D#+ Version:", ctx.Client.VersionString)
            .AddField("Guilds", ctx.Client.Guilds.Count.ToString(), true)
            .AddField("Uptime", date.Humanize(), true)
            .AddField("Ping", $"{ctx.Client.Ping} ms", true)
            .AddField("Add me", addMe);

        discordMessageBuilder.AddEmbed(discordEmbedBuilder.Build());
        discordMessageBuilder.AddComponents(new DiscordButtonComponent(ButtonStyle.Success, "feedback-button",
            "Feedback"));
        discordMessageBuilder.AsEphemeral();

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, discordMessageBuilder);
    }
}