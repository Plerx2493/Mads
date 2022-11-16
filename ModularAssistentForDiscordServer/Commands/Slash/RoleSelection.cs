using System.ComponentModel.DataAnnotations;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace MADS.Commands.Slash;

public class RoleSelection : ApplicationCommandModule
{
    [SlashCommand("RoleSelection",
        "Use this command in the channel the message should be posted"), SlashRequirePermissions(Permissions.Administrator)]
    public async Task RoleSelectionCreation(InteractionContext ctx)
    {
        //shows we are processing
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral());

        //check if the command was triggered in a guild and if so abort
        if (ctx.Guild is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Only possible in guilds"));
            return;
        }

        //check if the user has the required permissons
        if (!ctx.Member.Permissions.HasPermission(Permissions.ManageRoles))
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("You dont have the permission to use this command (ManageRoles)"));
            return;
        }

        //get all roles and Create a list of select menu options
        var options = Enumerable.Empty<DiscordSelectComponentOption>();
        var roles = ctx.Guild!.Roles.Values.ToList();
        
        //remove all roles from bots etc
        roles.RemoveAll(x => x.IsManaged);
        roles.RemoveAll(x => x.Position >= ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id).Result.Hierarchy);
        options = roles    
                  .Select(discordRole => new DiscordSelectComponentOption(discordRole.Name, discordRole.Id.ToString()))
                  .Aggregate(options, (current, option) => current.Append(option))
                  .ToList();

        //Create the select component and update our first response
        DiscordSelectComponent select = new("roleSelectionStart-" + ctx.Channel.Id,
        "Select your roles", options, false, 0, options.Count());
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddComponents(select));

        //Get the initial response an wait for a component interaction
        var response = await ctx.GetOriginalResponseAsync();
        var selectResponse = await response.WaitForSelectAsync(ctx.Member, "roleSelectionStart-" + ctx.Channel.Id, TimeSpan.FromSeconds(60));

        //Notify the user when the interaction times out and abort
        if (selectResponse.TimedOut)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder() { Content = "Timed out" });
            return;
        }

        //acknowledge interaction and edit first response to delete the select menu
        await selectResponse.Result.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Success").AsEphemeral());
        await ctx.EditResponseAsync(new DiscordWebhookBuilder() { Content = "Submitted" });

        /*
         * 1. get role ids from interaction
         * 2. get roles from guild
         * 3. create select menu options
         * 4. aggregate all and form a list
         */
        List<DiscordSelectComponentOption> selectedRoles = new();
        selectedRoles = selectResponse.Result.Values.Select(ulong.Parse)
                                                    .Select(roleId => ctx.Guild.GetRole(roleId))
                                                    .Select(role => new DiscordSelectComponentOption(role.Name, role.Id.ToString()))
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