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
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Humanizer;
using MADS.Services;

namespace MADS.Commands.Slash;

public sealed class About
{
    private readonly DiscordCommandService DiscordService;
    
    public About(DiscordCommandService service)
    {
        DiscordService = service;
    }
    
    [Command("about"), Description("Infos about the bot")]
    public async Task AboutCommand(CommandContext ctx)
    {
        DiscordEmbedBuilder discordEmbedBuilder = new();
        DiscordInteractionResponseBuilder discordMessageBuilder = new();
        string inviteUri = ctx.Client.CurrentApplication.GenerateOAuthUri(null, DiscordPermissions.Administrator,
            DiscordOAuthScope.Bot,
            DiscordOAuthScope.ApplicationsCommands);
        string addMe = $"[Click here!]({inviteUri.Replace(" ", "%20")})";
        
        TimeSpan diff = DateTime.Now - DiscordService.StartTime;
        string date = $"{diff.Days} days {diff.Hours} hours {diff.Minutes} minutes";
        
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
            //.AddField("Ping", $"{ctx.Client.Ping} ms", true)
            .AddField("Add me", addMe);
        
        discordMessageBuilder.AddEmbed(discordEmbedBuilder.Build());
        discordMessageBuilder.AddComponents(new DiscordButtonComponent(DiscordButtonStyle.Success, "feedback-button",
            "Feedback"));
        discordMessageBuilder.AsEphemeral();
        
        await ctx.RespondAsync(discordMessageBuilder);
    }
}