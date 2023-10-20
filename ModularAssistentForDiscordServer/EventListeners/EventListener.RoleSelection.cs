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
using MADS.Services;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MADS.EventListeners;

internal static partial class EventListener
{
    internal static void EnableRoleSelectionListener(DiscordClient client)
    {
        client.ComponentInteractionCreated += async Task (_, e) =>
        {
            if (e.Guild is null) return;

            if (e.Id != "RoleSelection:" + e.Guild.Id) return;
            
            //TODO Test if "await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);" is possible
            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().WithContent("Roles granted/revoked").AsEphemeral());

            var member = await e.Guild.GetMemberAsync(e.User.Id);
            var roles = e.Values.Select(ulong.Parse).Select(x => e.Guild.GetRole(x)).ToList();

            var newRoles = new List<DiscordRole>();
            newRoles.AddRange(roles);
            newRoles.RemoveAll(x => member.Roles.Contains(x));

            var oldRoles = new List<DiscordRole>();
            oldRoles.AddRange(roles);
            oldRoles.RemoveAll(x => !member.Roles.Contains(x));

            foreach (var role in newRoles) await member.GrantRoleAsync(role);
            foreach (var role in oldRoles) await member.RevokeRoleAsync(role);
        };
    }
}