using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MADS;
using MADS.Modules;

namespace MADS.Modules
{
    internal class ModerationModul : IMadsModul
    {
        public ModularDiscordBot ModularDiscordClient { get; set; }
        public List<ulong> GuildsEnabled { get; set; }
        public string ModulName { get; set; }
        public string ModulDescription { get; set; }
        public string[] Commands { get; set; }
        public Dictionary<string, string> CommandDescriptions { get; set; }
        public Type CommandClass { get; set; }
        public Type SlashCommandClass { get; set; }
        public DiscordIntents RequiredIntents { get; set; }
        public bool IsHidden { get; init; }

        public ModerationModul(ModularDiscordBot bot)
        {
            ModulName = "Moderation";
            ModulDescription = "Moderation commands";
            Commands = new string[] { "kick", "ban", "mute", "unmute", "purge", "warn", "unwarn", "warnlist", "warnlevel", "warnlevelset", "warnlevelreset", "moderation test", "test" };
            CommandDescriptions = new Dictionary<string, string>
            {
                { "kick", "Kicks a user from the server" },
                { "ban", "Bans a user from the server" },
                { "mute", "Mutes a user" },
                { "unmute", "Unmutes a user" },
                { "purge", "Deletes a number of messages" },                    //TODO: Implement command
                { "warn", "Warns a user" },                                     //TODO: Implement command
                { "unwarn", "Unwarns a user" },                                 //TODO: Implement command
                { "warnlist", "Shows the warns of a user" },                    //TODO: Implement command
                { "warnlevel", "Shows the warn level of a user" },              //TODO: Implement command
                { "warnlevelset", "Sets the warn level of a user" },            //TODO: Implement command
                { "warnlevelreset", "Resets the warn level of a user" }         //TODO: Implement command
            };
            CommandClass = typeof(ModerationCommands);
            SlashCommandClass = typeof(ModerationSlashCommands);
            RequiredIntents = 0;
            ModularDiscordClient = bot;
            IsHidden = false;
        }
    }
    
    internal class ModerationCommands : BaseCommandModule
    {
        public ModularDiscordBot ModularDiscordClient { get; set; }

        [GuildIsEnabled("Moderation"), Command("kick"), Description("Kicks a user from the server"), RequireBotPermissions(Permissions.KickMembers)]
        public async Task Kick(CommandContext ctx, DiscordUser user, [RemainingText] string reason = null)
        {
            //check if user has permission to kick member
            if (!ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.KickMembers))
            {
                await ctx.RespondAsync("You do not have the permission to kick users");
                return;
            }

            //Get member who should be kicked
            var userToKick = await ctx.Guild.GetMemberAsync(user.Id);

            //Check if user is valid
            if (userToKick is null)
            {
                await ctx.RespondAsync($"{user.Username} is not a valid user or is not on this guild");
                return;
            }

            //Check if user has a higher or equal hierarchy than the user who is trying to kick
            if (ctx.Member.Hierarchy <= userToKick.Hierarchy)
            {
                await ctx.RespondAsync("You cannot kick a user with a higher or equal hierarchy than you");
                return;
            }
            
            //excecute kick
            await userToKick.BanAsync(reason: reason);
            await ctx.RespondAsync($"{userToKick.DisplayName} has been kicked from the server");
        }

        [Command("ban"), Description("Bans a user from the server"), GuildIsEnabled("Moderation"), RequireBotPermissions(Permissions.BanMembers)]
        public async Task Ban(CommandContext ctx, DiscordMember user, [RemainingText] string reason = null)
        {
            //check if user has permission to ban member
            if (!ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.KickMembers))
            {
                await ctx.RespondAsync("You do not have the permission to ban users");
                return;
            }

            //Get member who should be banned
            var userToBan = await ctx.Guild.GetMemberAsync(user.Id);

            //Check if user is valid
            if (userToBan is null)
            {
                await ctx.RespondAsync($"{user.Username} is not a valid user or is not on this guild");
                return;
            }

            //Check if user has a higher or equal hierarchy than the user who is trying to ban
            if (ctx.Member.Hierarchy <= userToBan.Hierarchy)
            {
                await ctx.RespondAsync("You cannot ban a user with a higher or equal hierarchy than you");
                return;
            }

            //excecute ban
            await userToBan.BanAsync(reason: reason);
            await ctx.RespondAsync($"{userToBan.DisplayName} has been kicked from the server");
        }

        [Command("mute"), Description("Mutes a user in a server"), GuildIsEnabled("Moderation"), RequireBotPermissions(Permissions.ModerateMembers)]
        public async Task Mute(CommandContext ctx, DiscordMember user, int TimeInMinutes = 60 , [RemainingText] string reason = null)
        {
            if (!ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.ModerateMembers))
            {
                await ctx.RespondAsync("You do not have the permission to mute users");
                return;
            }

            var userToMute = await ctx.Guild.GetMemberAsync(user.Id);

            if (userToMute is null)
            {
                await ctx.RespondAsync($"{user.Username} is not a valid user or is not on this guild");
                return;
            }

            if (ctx.Member.Hierarchy <= userToMute.Hierarchy)
            {
                await ctx.RespondAsync("You cannot mute a user with a higher or equal hierarchy than you");
                return;
            }
            
            await user.TimeoutAsync(DateTimeOffset.Now.AddMinutes(TimeInMinutes), reason);
            await ctx.RespondAsync($"{user.Username} has been muted");
        }

        [Command("unmute"), Description("Unmutes a user in a server"), GuildIsEnabled("Moderation"), RequireBotPermissions(Permissions.ModerateMembers)]
        public async Task Unmute(CommandContext ctx, DiscordMember user, [RemainingText] string reason = null)
        {
            if (!ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.ModerateMembers))
            {
                await ctx.RespondAsync("You do not have the permission to unmute users");
                return;
            }

            var userToUnmute = await ctx.Guild.GetMemberAsync(user.Id);

            if (userToUnmute is null)
            {
                await ctx.RespondAsync($"{user.Username} is not a valid user or is not on this guild");
                return;
            }

            if (ctx.Member.Hierarchy <= userToUnmute.Hierarchy)
            {
                await ctx.RespondAsync("You cannot unmute a user with a higher or equal hierarchy than you");
                return;
            }

            await user.TimeoutAsync(null, reason);
            await ctx.RespondAsync($"{user.Username} has been unmuted");
        }

        [Command("warn"), Description("Warns a user in a server"), GuildIsEnabled("Moderation"), RequireBotPermissions(Permissions.KickMembers)]
        public async Task Warn(CommandContext ctx, DiscordMember user, [RemainingText] string reason = null)
        {
            if (!ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.KickMembers))
            {
                await ctx.RespondAsync("You do not have the permission to warn users");
                return;
            }

            var userToWarn = await ctx.Guild.GetMemberAsync(user.Id);

            if (userToWarn is null)
            {
                await ctx.RespondAsync($"{user.Username} is not a valid user or is not on this guild");
                return;
            }

            if (ctx.Member.Hierarchy <= userToWarn.Hierarchy)
            {
                await ctx.RespondAsync("You cannot warn a user with a higher or equal hierarchy than you");
                return;
            }

            await ctx.RespondAsync($"{user.Username} has been warned");
        }
    }

    [SlashCommandGroup("Moderation", "Commands to moderate a guild")]
    [GuildOnly]
    internal class ModerationSlashCommands : ApplicationCommandModule
    {
        [SlashCommand("test","test smth")]
        public async Task TestCommand(InteractionContext ctx)
        {
            var member = await ctx.Guild.GetMemberAsync(ctx.User.Id + 12313);

            var DiscordEmbed = new DiscordEmbedBuilder
            {
                Title = "Test",
                Description = $"Test executed",
                Color = DiscordColor.Blue,
                Timestamp = DateTime.Now,
            };


            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(DiscordEmbed));
        }
    }
}
