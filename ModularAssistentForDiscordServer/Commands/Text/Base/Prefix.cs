using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MADS.Entities;
using Microsoft.EntityFrameworkCore;

namespace MADS.Commands.Text.Base;

internal class Prefix : BaseCommandModule
{
    public        MadsServiceProvider            CommandService { get; set; }
    public        IDbContextFactory<MadsContext> DbFactory      { get; set; }

    [Command("prefix"), Description("Get the bot prefix for this server"), Cooldown(1, 30, CooldownBucketType.Channel), RequireGuild]
    public async Task GetPrefix(CommandContext ctx)
    {
        var dbContext = await DbFactory.CreateDbContextAsync();
        GuildConfigDbEntity config;
        
        if (dbContext.Guilds.Any(x => x.Id == ctx.Guild.Id))
        {
            config = dbContext.Guilds.First(x => x.Id == ctx.Guild.Id).Config;
        }
        else
        {
            config = dbContext.Guilds.First(x => x.Id == 0).Config;
        }
        
        await ctx.RespondAsync("Current prefix is: `" + config.Prefix + "`");
        
        dbContext.Dispose();
    }
    
    [Command("setprefix"), Description("Set the bot prefix for this server"),
     RequirePermissions(Permissions.ManageGuild), RequireGuild]
    public async Task SetPrefix(CommandContext ctx, [Description("The new prefix")] string prefix)
    {
        var dbContext = await DbFactory.CreateDbContextAsync();
        GuildConfigDbEntity config;
        
        if (dbContext.Guilds.Any(x => x.Id == ctx.Guild.Id))
        {
            config = dbContext.Guilds.First(x => x.Id == ctx.Guild.Id).Config;
        }
        else
        {
            config = dbContext.Guilds.First(x => x.Id == 0).Config;
        }

        config.Prefix = prefix;
        
        await dbContext.SaveChangesAsync();

        await ctx.Guild.CurrentMember.ModifyAsync(x => x.Nickname = ctx.Guild.CurrentMember.Username + $" [{prefix}]");

        await ctx.RespondAsync($"New prefix is: `{prefix}`");
        
        dbContext.Dispose();
    }
}