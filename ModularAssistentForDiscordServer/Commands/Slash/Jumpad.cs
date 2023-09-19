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
using MADS.CustomComponents;
using MADS.Extensions;

namespace MADS.Commands.Slash;

[GuildOnly]
public sealed class Jumppad : MadsBaseApplicationCommand
{
    [SlashCommand("jumppad", "Create a jumppad button"), SlashCommandPermissions(Permissions.MoveMembers)]
    public async Task Test
    (
        InteractionContext ctx,
        [Option("originChannel", "Channel where the users will be moved out"), ChannelTypes(ChannelType.Voice)]
        DiscordChannel originChannel,
        [Option("targetChannel", "Channel where the users will be put in"), ChannelTypes(ChannelType.Voice)]
        DiscordChannel targetChannel
    )
    {
        DiscordInteractionResponseBuilder message = new();
        DiscordButtonComponent newButton = new(ButtonStyle.Success, "Placeholder", "Jump");
        newButton = newButton.AsActionButton(
            ActionDiscordButtonEnum.MoveVoiceChannel,
            originChannel.Id, targetChannel.Id);

        message.AddComponents(newButton);
        message.Content = "Jumppad";
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, message);
    }
}