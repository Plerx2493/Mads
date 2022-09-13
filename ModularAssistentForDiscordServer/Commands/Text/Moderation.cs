using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace MADS.Commands.Text;

internal class ModerationCommands : BaseCommandModule
{
    public MadsServiceProvider CommandService { get; set; }

    [Command("kick"), Description("Kicks a user from the server"),
     RequireBotPermissions(Permissions.KickMembers), RequireGuild]
    public async Task Kick(CommandContext ctx, DiscordMember user, [RemainingText] string reason = null)
    {
        //check if user has permission to kick member
        if (!ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.KickMembers))
        {
            await ctx.RespondAsync("You do not have the permission to kick users");
            return;
        }

        //Check if user has a higher or equal hierarchy than the user who is trying to kick
        if (ctx.Member.Hierarchy <= user.Hierarchy)
        {
            await ctx.RespondAsync("You cannot kick a user with a higher or equal hierarchy than you");
            return;
        }

        //execute kick
        await user.RemoveAsync(reason: reason);
        await ctx.RespondAsync($"{user.DisplayName} has been kicked from the server");
    }

    [Command("ban"), Description("Bans a user from the server"),
     RequireBotPermissions(Permissions.BanMembers), RequireGuild]
    public async Task Ban(CommandContext ctx, DiscordMember user, [RemainingText] string reason = null)
    {
        //check if user has permission to ban member
        if (!ctx.Member!.PermissionsIn(ctx.Channel).HasPermission(Permissions.KickMembers))
        {
            await ctx.RespondAsync("You do not have the permission to ban users");
            return;
        }

        //Check if user has a higher or equal hierarchy than the user who is trying to ban
        if (ctx.Member.Hierarchy <= user.Hierarchy)
        {
            await ctx.RespondAsync("You cannot ban a user with a higher or equal hierarchy than you");
            return;
        }

        //excecute ban
        await user.BanAsync(reason: reason);
        await ctx.RespondAsync($"{user.DisplayName} has been kicked from the server");
    }

    [Command("mute"), Description("Mutes a user in a server"),
     RequireBotPermissions(Permissions.ModerateMembers), RequireGuild]
    public async Task Mute(CommandContext ctx, DiscordMember user, int timeInMinutes = 60,
        [RemainingText] string reason = null)
    {
        if (!ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.ModerateMembers))
        {
            await ctx.RespondAsync("You do not have the permission to mute users");
            return;
        }

        if (ctx.Member.Hierarchy <= user.Hierarchy)
        {
            await ctx.RespondAsync("You cannot mute a user with a higher or equal hierarchy than you");
            return;
        }

        await user.TimeoutAsync(DateTimeOffset.Now.AddMinutes(timeInMinutes), reason);
        await ctx.RespondAsync($"{user.Username} has been muted");
    }

    [Command("unmute"), Description("Unmutes a user in a server"),
     RequireBotPermissions(Permissions.ModerateMembers), RequireGuild]
    public async Task Unmute(CommandContext ctx, DiscordMember user, [RemainingText] string reason = null)
    {
        if (!ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.ModerateMembers))
        {
            await ctx.RespondAsync("You do not have the permission to unmute users");
            return;
        }

        if (ctx.Member.Hierarchy <= user.Hierarchy)
        {
            await ctx.RespondAsync("You cannot unmute a user with a higher or equal hierarchy than you");
            return;
        }

        await user.TimeoutAsync(null, reason);
        await ctx.RespondAsync($"{user.Username} has been unmuted");
    }

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

    [Command("purge"), Description("Purges messages"),
     RequireBotPermissions(Permissions.ManageMessages), RequireGuild]
    public async Task Purge(CommandContext ctx, int amount = 99)
    {
        if (amount + 1 > 100)
        {
            await ctx.RespondAsync("You cannot purge more than 99 messages at once");
            return;
        }

        var messagesApi = await ctx.Channel.GetMessagesAsync(amount + 1);
        List<DiscordMessage> messages = new();
        messages.AddRange(messagesApi);

        messages.RemoveAll(x => (DateTime.UtcNow - x.Timestamp).TotalDays > 14);

        await ctx.Channel.DeleteMessagesAsync(messages);

        var response = await ctx.Channel.SendMessageAsync(
            $"{messages.Count} messages deleted {DiscordEmoji.FromName(CommandService.ModularDiscordBot.DiscordClient, ":Bussi:")}");

        await Task.Delay(10000);

        await response.DeleteAsync();
    }

    [Command("scan"), Description("Scan the entire guild if there are silent raids"),
     RequirePermissions(Permissions.Administrator), RequireGuild /*Cooldown(1, 28_800, CooldownBucketType.Guild)*/]
    public async Task Scan(CommandContext ctx)
    {
        var members = await ctx.Guild.GetAllMembersAsync();

        var groupByLastNamesQuery =
            from member in members
            group member by member.JoinedAt into newGroup
            orderby newGroup.Key
            select newGroup;

        foreach (var memberGroup in groupByLastNamesQuery)
        {
            Console.WriteLine("Gruppe: " + memberGroup.Key);
            foreach (var item in memberGroup)
            {
                Console.WriteLine(item.Username);
            }
        }
    }
}