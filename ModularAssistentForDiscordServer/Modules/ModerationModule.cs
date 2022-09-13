using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace MADS.Modules;

internal class ModerationModule : IMadsModul
{
    public List<ulong>                GuildsEnabled        { get; set; }
    public ModularDiscordBot          ModularDiscordClient { get; set; }
    public string                     ModuleName           { get; set; }
    public string                     ModuleDescription    { get; set; }
    public string[]                   Commands             { get; set; }
    public Dictionary<string, string> CommandDescriptions  { get; set; }
    public Type                       CommandClass         { get; set; }
    public Type                       SlashCommandClass    { get; set; }
    public DiscordIntents             RequiredIntents      { get; set; }
    public bool                       IsHidden             { get; init; }
    
    public ModerationModule(ModularDiscordBot bot)
    {
        ModuleName = "Moderation";
        ModuleDescription = "Moderation commands";
        Commands = new[]
        {
            "kick", "ban", "mute", "unmute", "purge", "warn", "unwarn", "warnlist", "warnlevel", "warnlevelset",
            "warnlevelreset", "moderation test", "test"
        };
        CommandDescriptions = new Dictionary<string, string>
        {
            { "kick", "Kicks a user from the server" },
            { "ban", "Bans a user from the server" },
            { "mute", "Mutes a user" },
            { "unmute", "Unmutes a user" },
            { "purge", "Deletes a number of messages" },
            { "warn", "Warns a user" },                             //TODO: Implement warning system
            { "unwarn", "Unwarns a user" },                         //TODO: Implement warning system
            { "warnlist", "Shows the warns of a user" },            //TODO: Implement warning system
            { "warnlevel", "Shows the warn level of a user" },      //TODO: Implement warning system
            { "warnlevelset", "Sets the warn level of a user" },    //TODO: Implement warning system
            { "warnlevelreset", "Resets the warn level of a user" } //TODO: Implement warning system
        };
        CommandClass = typeof(ModerationCommands);
        SlashCommandClass = typeof(ModerationSlashCommands); //TODO: Fix
        RequiredIntents = 0;
        ModularDiscordClient = bot;
        IsHidden = false;
    }
}

internal class ModerationCommands : BaseCommandModule
{
    public MadsServiceProvider CommandService { get; set; }

    [GuildIsEnabled("Moderation"), Command("kick"), Description("Kicks a user from the server"),
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

    [Command("ban"), Description("Bans a user from the server"), GuildIsEnabled("Moderation"),
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

    [Command("mute"), Description("Mutes a user in a server"), GuildIsEnabled("Moderation"),
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

    [Command("unmute"), Description("Unmutes a user in a server"), GuildIsEnabled("Moderation"),
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

    [Command("warn"), Description("Warns a user in a server"), GuildIsEnabled("Moderation"),
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

    [Command("purge"), Description("Purges messages"), GuildIsEnabled("Moderation"),
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

[SlashCommandGroup("Moderation", "Commands to moderate a guild"), GuildOnly]
internal class ModerationSlashCommands : ApplicationCommandModule
{
    [SlashCommand("test", "test smth")]
    public async Task TestCommand(InteractionContext ctx)
    {
        /*
        var member = await ctx.Guild.GetMemberAsync(ctx.User.Id + 12313);

        var discordEmbed = new DiscordEmbedBuilder
        {
            Title = "Test",
            Description = $"Test executed",
            Color = DiscordColor.Blue,
            Timestamp = DateTime.Now,
        };

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(discordEmbed));
        */
        var tmp = await ctx.Client.GetGlobalApplicationCommandsAsync();
        foreach (var cmd in tmp)
        {
            await ctx.Client.DeleteGlobalApplicationCommandAsync(cmd.Id);
        }

        var discordEmbed = new DiscordEmbedBuilder
        {
            Title = "Test",
            Description = "Test executed",
            Color = DiscordColor.Blue,
            Timestamp = DateTime.Now
        };

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AddEmbed(discordEmbed));
    }

    /*
    [SlashCommand("freeze", "Freezes a conversation in a moderation channel")]
    public async Task FreezeCommand(InteractionContext ctx, [Option("number_of_messages", "Number of messages which should be freezed")] long messages, [Option("channel", "Channel where the messages should be freezed")] DiscordChannel discordChannel = null)
    {
    }
    */
}