﻿// Copyright 2023 Plerx2493
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
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using MADS.Extensions;

namespace MADS.Commands.Slash;

public sealed class RoleSelection
{
    [Command("RoleSelection"), Description("Use this command in the channel the message should be posted"),
     RequirePermissions(DiscordPermissions.ManageRoles),
     RequireGuild]
    public async Task RoleSelectionCreation
    (
        CommandContext ctx,
        [Description("Message which should be above the menu")]
        string messageContent = ""
    )
    {
        //show we are processing
        await ctx.DeferAsync(true);
        
        //check if the user has the required permissons
        if (ctx.Member is null || !ctx.Member.Permissions.HasPermission(DiscordPermissions.ManageRoles))
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent(
                    "You dont have the permission to use this command (ManageRoles)"));
            return;
        }
        
        //get all roles and Create a list of select menu options
        IEnumerable<DiscordSelectComponentOption> options = [];
        List<DiscordRole> roles = ctx.Guild!.Roles.Values.ToList();
        
        //remove all roles from bots etc
        roles.RemoveAll(x => x.IsManaged);
        roles.RemoveAll(x => x.Position >= ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id).Result.Hierarchy);
        roles.RemoveAll(x => x.Position >= ctx.Member.Hierarchy);
        options = roles
            .Select(discordRole => new DiscordSelectComponentOption(discordRole.Name, discordRole.Id.ToString()))
            .Aggregate(options, (current, option) => current.Append(option))
            .ToList();
        
        //Create the select component and update our first response
        DiscordSelectComponent select = new("roleSelectionStart-" + ctx.Channel.Id,
            "Select your roles", options, false, 0, options.Count());
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddComponents(select));
        
        //Get the initial response an wait for a component interaction
        DiscordMessage? response = await ctx.GetResponseAsync();
        InteractivityResult<ComponentInteractionCreatedEventArgs> selectResponse = await response!.WaitForSelectAsync(
            ctx.Member, "roleSelectionStart-" + ctx.Channel.Id,
            TimeSpan.FromSeconds(60 * 3));
        
        //Notify the user when the interaction times out and abort
        if (selectResponse.TimedOut)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder
            {
                Content = "Timed out (180 seconds)"
            });
            return;
        }
        
        //acknowledge interaction and edit first response to delete the select menu
        await selectResponse.Result.Interaction.CreateResponseAsync(
            DiscordInteractionResponseType.DeferredMessageUpdate);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder
        {
            Content = "Submitted"
        });
        
        /*
         * 1. get role ids from interaction
         * 2. get roles from guild
         * 3. create select menu options
         * 4. aggregate all and form a list
         */
        List<DiscordSelectComponentOption> selectedRoles = new();
        selectedRoles = selectResponse.Result.Values.Select(ulong.Parse)
            .Select(roleId => ctx.Guild.GetRole(roleId))
            .Where(x => x is not null)
            .Select(role => new DiscordSelectComponentOption(role!.Name, role.Id.ToString()))
            .Aggregate(selectedRoles, (current, option) => current.Append(option).ToList());
        
        //Create the final select menu and send it in the channel
        DiscordSelectComponent finalSelect = new("RoleSelection:" + ctx.Guild.Id,
            "Select your roles", selectedRoles, false, 0, selectedRoles.Count);
        DiscordMessageBuilder finalResponse = new();
        finalResponse
            .AddComponents(finalSelect)
            .Content = "Chose your roles";
        await ctx.Channel.SendMessageAsync(finalResponse);
    }
}