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
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using MADS.CustomComponents;
using Quartz.Util;

namespace MADS.Commands.Slash;

public sealed class Jumppad
{
    [Command("jumppad"), Description("Create a jumppad button"), RequireGuild,
     RequirePermissions(DiscordPermissions.MoveMembers)]
    public async Task Test
    (
        CommandContext ctx,
        [Description("Channel where the users will be moved out"),
         ChannelTypes(DiscordChannelType.Voice, DiscordChannelType.Stage)]
        DiscordChannel originChannel,
        [Description("Channel where the users will be put in"),
         ChannelTypes(DiscordChannelType.Voice, DiscordChannelType.Stage)]
        DiscordChannel targetChannel,
        [Description("Message to be sent")] string? content = null
    )
    {
        DiscordInteractionResponseBuilder message = new();
        DiscordButtonComponent newButton = new(DiscordButtonStyle.Success, "Placeholder", "Jump");
        newButton = newButton.AsActionButton(
            ActionDiscordButtonEnum.MoveVoiceChannel,
            originChannel.Id, targetChannel.Id);
        
        message.AddComponents(newButton);
        message.WithContent(!content.IsNullOrWhiteSpace() ? content! : "Jumppad");
        await ctx.RespondAsync(message);
    }
}