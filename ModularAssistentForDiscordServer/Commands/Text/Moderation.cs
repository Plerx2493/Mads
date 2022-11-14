using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace MADS.Commands.Text;

public class ModerationCommands : BaseCommandModule
{
    public MadsServiceProvider CommandService { get; set; }
    
    //TODO Develope Warn system
    
    /*
    [Command("warn"), Description("Warns a user in a server"),
     RequireBotPermissions(Permissions.KickMembers), RequireGuild]
    public async Task Warn(CommandContext ctx, DiscordMember user, [RemainingText] string reason = null)
    {
        if (!ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.KickMembers))
        {
            await ctx.RespondAsync("You do not have the permission to warn users");
            return;
        }

        if (ctx.Member.Hierarchy <= user.Hierarchy)
        {
            await ctx.RespondAsync("You cannot warn a user with a higher or equal hierarchy than you");
            return;
        }

        await ctx.RespondAsync($"{user.Username} has been warned");
    }
    */
}