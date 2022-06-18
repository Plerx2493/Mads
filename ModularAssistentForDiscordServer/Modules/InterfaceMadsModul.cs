using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext.Attributes;
using MADS.Extensions;

namespace MADS.Modules
{
    public interface IMadsModul
    {
        /// <summary>
        /// ModularDiscordBot to which the module is attached
        /// </summary>
        ModularDiscordBot ModularDiscordClient { get; set; }
        /// <summary>
        /// Name of the module
        /// </summary>
        string ModulName { get; set; }
        /// <summary>
        /// Description of the module
        /// </summary>
        string ModulDescription { get; set; }
        /// <summary>
        /// Array of commandnames
        /// </summary>
        string[] Commands { get; set; }
        /// <summary>
        /// Dictionary in which the commanddescription is matched to the commandnames
        /// </summary>
        Dictionary<string, string> CommandDescriptions { get; set; }
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        /// <summary>
        /// Class which contains the CNext commands of the module
        /// </summary>
        Type? CommandClass { get; set; }
        /// <summary>
        /// Class which contains the commands and context menu buttons of the module
        /// </summary>
        Type? SlashCommandClass { get; set; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        /// <summary>
        /// Required DiscordIntents for the module 
        /// </summary>
        DiscordIntents RequiredIntents { get; }
        /// <summary>
        /// Represents if the module is shown to the user
        /// </summary>
        bool IsHidden { get; init; }

        /// <summary>
        /// Enable this module in given guild
        /// </summary>
        /// <param name="guildId">Id of the guild where the module should be enabled</param>
        /// <param name="updateSlashies">Set if the slash commands should be refreshed. Set to false if methode is used in a loop</param>
        public void Enable(ulong guildId, bool updateSlashies = true)
        {
            if(ModularDiscordClient.GuildSettings.TryGetValue(guildId, out _))
            {
                ModularDiscordClient.GuildSettings[guildId].AktivModules.Add(ModulName);
            }
            else
            {
                ModularDiscordClient.GuildSettings.Add(guildId, new());
                ModularDiscordClient.GuildSettings[guildId].AktivModules.Add(ModulName);
            }

            RegisterCommands(guildId, updateSlashies);
        }

        /// <summary>
        /// Register all commands of this module
        /// </summary>
        /// <param name="guildId">Id of the guild where the commands should be active</param>
        /// <param name="updateSlashies">Set if the slash commands should be refreshed. Set to false if methode is used in a loop</param>
        public void RegisterCommands(ulong guildId, bool updateSlashies = true)
        {
            if (ModularDiscordClient.ModulesAktivGuilds.TryGetValue(ModulName, out _))
            {
                ModularDiscordClient.ModulesAktivGuilds[ModulName].Add(guildId);
            }
            else
            {
                List<ulong> newList = new()
                {
                    guildId
                };
                ModularDiscordClient.ModulesAktivGuilds[ModulName] = newList;
            }

            if (SlashCommandClass is not null && typeof(ApplicationCommandModule).IsAssignableFrom(SlashCommandClass))
            {
                Console.WriteLine("Slashies registered");
                ModularDiscordClient.SlashCommandsExtension.RegisterCommands(SlashCommandClass, guildId);
                if (updateSlashies) ModularDiscordClient.SlashCommandsExtension.RefreshCommands();
            }

            DataProvider.SetConfig(ModularDiscordClient.GuildSettings);
        }

        /// <summary>
        /// Registers the commands of this module
        /// </summary>
        public void RegisterCNext()
        {
            if (CommandClass is not null && typeof(BaseCommandModule).IsAssignableFrom(CommandClass))
            {
                ModularDiscordClient.CommandsNextExtension.RegisterCommands(CommandClass);
            }
        }

        /// <summary>
        /// Disable this module in given guild
        /// </summary>
        /// <param name="guildId">Id of the guild where this module should be disabled</param>
        /// <param name="updateSlashies">Set if the slash commands should be refreshed. Set to false if methode is used in a loop and refresh manuel after the loop</param>
        public void Disable(ulong guildId, bool updateSlashies = true)
        {
            ModularDiscordClient.ModulesAktivGuilds[ModulName].RemoveAll(x => x == guildId);

            var aktivCommandsList = ModularDiscordClient.SlashCommandsExtension.RegisteredCommands;
            var aktivCommandsDict = aktivCommandsList.ToDictionary(x => x.Key, x => x.Value);

            List<DiscordApplicationCommand> CommandList = new();
            if (aktivCommandsDict.TryGetValue(guildId, out IReadOnlyList<DiscordApplicationCommand> ReadonlyCommands))
            {
                CommandList = ReadonlyCommands.ToList();
                CommandList.RemoveAll(x => Commands.Contains(x.Name));    
            }
            ModularDiscordClient.DiscordClient.BulkOverwriteGuildApplicationCommandsAsync(guildId, CommandList);
            if (updateSlashies) ModularDiscordClient.SlashCommandsExtension.RefreshCommands();

            DataProvider.SetConfig(ModularDiscordClient.GuildSettings);
        }
    }
    
    internal class MadsServiceProvider
    {
        public ModularDiscordBot modularDiscordBot;
        //-> ModuleName -> List of guildIds where the module is activ
        public Dictionary<string, List<ulong>> modulesActivGuilds;

        public MadsServiceProvider(ModularDiscordBot modularDiscordBot, Dictionary<string, List<ulong>> modulesActivGuilds)
        {
            this.modularDiscordBot = modularDiscordBot;
            this.modulesActivGuilds = modulesActivGuilds;
        }
    }

    /// <summary>
    /// Atribute for modules to check if the module is enabled in given guild
    /// </summary>
    internal class GuildIsEnabled : CheckBaseAttribute
    {
        /// <summary>
        /// Name of the module
        /// </summary>
        readonly string ModulName;
        public GuildIsEnabled(string modulName)
        {
            ModulName = modulName;
        }

        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            MadsServiceProvider services = (MadsServiceProvider) ctx.CommandsNext.Services.GetService(typeof(MadsServiceProvider));

            if (services.modulesActivGuilds.TryGetValue(ModulName, out List<ulong> guilds))
            {
                if (guilds.Contains(ctx.Guild.Id))
                {
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Interface to get a standard DiscordEmbedBuilder
    /// </summary>
    internal interface IMadsCommandBase
    {
        /// <summary>
        /// Get a standard DiscordEmbedBuilder with given title and message
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        internal DiscordEmbedBuilder GetStandardEmbed(CommandContext ctx, string title, string message)
        {
            DiscordEmbedBuilder discordEmbedBuilder = new();
            discordEmbedBuilder
                .WithAuthor(ctx.Member.Nickname, ctx.Member.AvatarUrl, ctx.Member.AvatarUrl)
                .WithColor(new(88, 101, 242))
                .WithFooter("MADS ")
                .WithTimestamp(DateTime.Now)
                .WithTitle(title)
                .WithDescription(message);

            return discordEmbedBuilder;
        }
    }
}
