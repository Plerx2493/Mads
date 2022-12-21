﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MADS.Extensions;

namespace MADS.Commands.Text.Base;

public class ExitGuild : MadsBaseCommand
{
    [Command("exit"), Description("Exit the bot"), RequirePermissions(Permissions.ManageGuild), RequireGuild]
    public static async Task ExitGuildCommand(CommandContext ctx)
    {
        await ctx.RespondAsync("Leaving server...");
        await ctx.Guild.LeaveAsync();
    }

    [Command("leave"), Description("Leave given server"), RequireGuild, Hidden, RequireOwner]
    public static async Task LeaveGuildOwner(CommandContext ctx)
    {
        await ctx.Message.DeleteAsync();
        await ctx.Guild.LeaveAsync();
    }
}