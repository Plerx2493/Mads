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
                { "unmute", "Unmutes a user" },
                { "purge", "Deletes a number of messages" },
                { "warn", "Warns a user" },
                { "unwarn", "Unwarns a user" },
                { "warnlist", "Shows the warns of a user" },
                { "warnlevel", "Shows the warn level of a user" },
                { "warnlevelset", "Sets the warn level of a user" },
                { "warnlevelreset", "Resets the warn level of a user" }
            };
            CommandClass = typeof(ModerationCommands);
            SlashCommandClass = typeof(ModerationSlashCommands);
            RequiredIntents = DiscordIntents.AllUnprivileged;
            ModularDiscordClient = bot;
        }
    }
    
    internal class ModerationCommands : BaseCommandModule
    {

        [GuildIsEnabled("Moderation")]
        [Command("kick")]
        [Description("Kicks a user from the server")]
        public async Task Kick(CommandContext ctx, DiscordUser user, [RemainingText] string reason = null)
        {
            if (ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.KickMembers))
            {
                var userToKick = await ctx.Guild.GetMemberAsync(user.Id);
                if (userToKick != null)
                {
                    await userToKick.BanAsync(reason: reason);
                    await ctx.RespondAsync($"{userToKick.Username} has been kicked from the server");
                }
                else
                {
                    await ctx.RespondAsync($"{user} is not a valid user");
                }
            }
            else
            {
                await ctx.RespondAsync("You do not have the permission to kick users");
            }

            
        }

        [Command("ban")]
        [Description("Bans a user from the server")]
        public async Task Ban(CommandContext ctx, DiscordMember userToBan, [RemainingText] string reason = null)
        {
            if (ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.BanMembers))
            {
                if (userToBan != null)
                {
                    await userToBan.BanAsync(reason: reason);
                    await ctx.RespondAsync($"{userToBan.Username} has been banned from the server");
                }
                else
                {
                    await ctx.RespondAsync($"{userToBan} is not a valid user");
                }
            }
            else
            {
                await ctx.RespondAsync("You do not have the permission to ban users");
            }
        }

        [Command("mute")]
        [Description("Mutes a user")]
        public async Task Mute(CommandContext ctx, DiscordMember user, int TimeInMinutes  , [RemainingText] string reason = null)
        {
            if ( ctx.Member.Hierarchy > user.Hierarchy)
            {
                await ctx.RespondAsync("You cannot mute a user with a higher or equal hierarchy");
                return;
            }

            if (!ctx.Member.PermissionsIn(ctx.Channel).HasPermission(Permissions.MuteMembers))
            {
                await ctx.RespondAsync("You do not have the permission to mute users");
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
