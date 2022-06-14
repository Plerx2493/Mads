using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MADS;
using MADS.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MADS.Modules
{
    internal class DevModule : IMadsModul
    {
        public ModularDiscordBot ModularDiscordClient { get; set; }
        public string ModulName { get; set; }
        public string ModulDescription { get; set; }
        public string[] Commands { get; set; }
        public Dictionary<string, string> CommandDescriptions { get; set; }
        public Type CommandClass { get; set; }
        public Type SlashCommandClass { get; set; }

        public DiscordIntents RequiredIntents { get; set; }

        public bool IsHidden { get; init; }

        public DevModule(ModularDiscordBot modularDiscordClient)
        {
            ModularDiscordClient = modularDiscordClient;
            ModulName = "Dev";
            ModulDescription = "";
            Commands = new string[] { "" };
            CommandDescriptions = new();
            CommandClass = typeof(DevCommands);
            SlashCommandClass = null;
            RequiredIntents = 0;
            IsHidden = true;
            
        }
    }

    internal class DevCommands : BaseCommandModule
    {
        public ModularDiscordBot ModularDiscordClient { get; set; }

        [Command("guild"), RequireOwner, GuildIsEnabled("Dev")]
        public async Task GetGuild(CommandContext ctx, ulong id)
        {
            await ctx.RespondAsync($"Guild: {ctx.Guild.Name}");
        }
    }
}
