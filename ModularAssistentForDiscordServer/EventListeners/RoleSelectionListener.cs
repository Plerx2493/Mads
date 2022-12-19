using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace MADS.EventListeners;

internal static partial class EventListener
{
    internal static void EnableRoleSelectionListener(DiscordClient client)
    {
        client.ComponentInteractionCreated += async Task(_, e) =>
        {
            if (e.Guild is null) return;

            if (e.Id != "RoleSelection:" + e.Guild.Id) return;

            client.Logger.LogDebug(new EventId(420, "MADS"), $"Roleselection triggered: [{e.Id}]");


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

            foreach (var role in newRoles)
            {
                await member.GrantRoleAsync(role);
            }
            foreach (var role in oldRoles)
            {
                await member.RevokeRoleAsync(role);
            }
        };
    }
}