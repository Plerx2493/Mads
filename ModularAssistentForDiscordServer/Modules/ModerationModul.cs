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

        public ModerationModul(ModularDiscordBot bot)
        {
            ModulName = "Moderation";
            ModulDescription = "Moderation commands";
            Commands = new string[] { "kick", "ban", "mute", "unmute", "purge", "warn", "unwarn", "warnlist", "warnlevel", "warnlevelset", "warnlevelreset" };
            CommandDescriptions = new Dictionary<string, string>
            {
                { "kick", "Kicks a user from the server" },
                { "ban", "Bans a user from the server" },
                { "mute", "Mutes a user" },                                     
                { "unmute", "Unmutes a user" },                                 //TODO
                { "purge", "Deletes a number of messages" },                    //TODO
                { "warn", "Warns a user" },                                     //TODO
                { "unwarn", "Unwarns a user" },                                 //TODO
                { "warnlist", "Shows the warns of a user" },                    //TODO
                { "warnlevel", "Shows the warn level of a user" },              //TODO
                { "warnlevelset", "Sets the warn level of a user" },            //TODO
                { "warnlevelreset", "Resets the warn level of a user" }         //TODO
            };
            CommandClass = typeof(ModerationCommands);
            SlashCommandClass = typeof(ModerationSlashCommands);
            RequiredIntents = DiscordIntents.AllUnprivileged;
            ModularDiscordClient = bot;

            if (CommandClass is not null && typeof(BaseCommandModule).IsAssignableFrom(CommandClass))
            {
                ModularDiscordClient.CommandsNextExtension.RegisterCommands(CommandClass);
            }
        }
    }
    
    internal class ModerationCommands : BaseCommandModule
    {
        [GuildIsEnabled("Moderation"), Command("kick"), Description("Kicks a user from the server"), RequireBotPermissions(Permissions.KickMembers)]
        public async Task Kick(CommandContext ctx, DiscordUser user, [RemainingText] string reason = null)
        {
            if (!ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.KickMembers))
            {
                await ctx.RespondAsync("You do not have the permission to kick users");
                return;
            }

            var userToKick = await ctx.Guild.GetMemberAsync(user.Id);

            if (userToKick is null)
            {
                await ctx.RespondAsync($"{user.Username} is not a valid user or is not on this guild");
                return;
            }

            if (ctx.Member.Hierarchy <= userToKick.Hierarchy)
            {
                await ctx.RespondAsync("You cannot kick a user with a higher or equal hierarchy than you");
                return;
            }
               
            await userToKick.BanAsync(reason: reason);
            await ctx.RespondAsync($"{userToKick.DisplayName} has been kicked from the server");
        }

        [Command("ban"), Description("Bans a user from the server"), GuildIsEnabled("Moderation"), RequireBotPermissions(Permissions.BanMembers)]
        public async Task Ban(CommandContext ctx, DiscordMember user, [RemainingText] string reason = null)
        {
            if (!ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.KickMembers))
            {
                await ctx.RespondAsync("You do not have the permission to ban users");
                return;
            }

            var userToBan = await ctx.Guild.GetMemberAsync(user.Id);

            if (userToBan is null)
            {
                await ctx.RespondAsync($"{user.Username} is not a valid user or is not on this guild");
                return;
            }

            if (ctx.Member.Hierarchy <= userToBan.Hierarchy)
            {
                await ctx.RespondAsync("You cannot ban a user with a higher or equal hierarchy than you");
                return;
            }

            await userToBan.BanAsync(reason: reason);
            await ctx.RespondAsync($"{userToBan.DisplayName} has been kicked from the server");
        }

        [Command("mute"), Description("Mutes a user in a server"), GuildIsEnabled("Moderation"), RequireBotPermissions(Permissions.ModerateMembers)]
        public async Task Mute(CommandContext ctx, DiscordMember user, int TimeInMinutes  , [RemainingText] string reason = null)
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
    }

    [SlashCommandGroup("Moderation", "")]
    [GuildOnly]
    internal class ModerationSlashCommands : ApplicationCommandModule
    {
        
    }
}
