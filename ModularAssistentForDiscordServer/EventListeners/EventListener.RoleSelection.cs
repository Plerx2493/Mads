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
using DSharpPlus.EventArgs;

namespace MADS.EventListeners;

internal static partial class EventListener
{
    internal static async Task OnRoleSelection(DiscordClient client, ComponentInteractionCreateEventArgs e)
    {
        if (e.Guild is null)
        {
            return;
        }

        if (e.Id != "RoleSelection:" + e.Guild.Id)
        {
            return;
        }

        //TODO Test if "await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);" is possible
        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent("Roles granted/revoked").AsEphemeral());

        DiscordMember member = await e.Guild.GetMemberAsync(e.User.Id);
        List<DiscordRole> roles = e.Values.Select(ulong.Parse).Select(x => e.Guild.GetRole(x)).ToList();

        List<DiscordRole> newRoles = [];
        newRoles.AddRange(roles);
        newRoles.RemoveAll(x => member.Roles.Contains(x));

        List<DiscordRole> oldRoles = [];
        oldRoles.AddRange(roles);
        oldRoles.RemoveAll(x => !member.Roles.Contains(x));

        foreach (DiscordRole role in newRoles)
        {
            await member.GrantRoleAsync(role);
        }

        foreach (DiscordRole role in oldRoles)
        {
            await member.RevokeRoleAsync(role);
        }
    }
}