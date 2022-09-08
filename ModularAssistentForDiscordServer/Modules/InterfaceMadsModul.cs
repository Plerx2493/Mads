using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
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
        string ModuleName { get; set; }

        /// <summary>
        /// Description of the module
        /// </summary>
        string ModuleDescription { get; set; }

        /// <summary>
        /// Array of command names
        /// </summary>
        string[] Commands { get; set; }

        /// <summary>
        /// Dictionary in which the command description is matched to the command name
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
            if (ModularDiscordClient.GuildSettings.TryGetValue(guildId, out _))
            {
                ModularDiscordClient.GuildSettings[guildId].AktivModules.Add(ModuleName);
            }
            else
            {
                ModularDiscordClient.GuildSettings.Add(guildId, new());
                ModularDiscordClient.GuildSettings[guildId].AktivModules.Add(ModuleName);
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
            if (ModularDiscordClient.ModulesActiveGuilds.TryGetValue(ModuleName, out _))
            {
                ModularDiscordClient.ModulesActiveGuilds[ModuleName].Add(guildId);
            }
            else
            {
                List<ulong> newList = new()
                {
                    guildId
                };
                ModularDiscordClient.ModulesActiveGuilds[ModuleName] = newList;
            }

            if (SlashCommandClass is not null && typeof(ApplicationCommandModule).IsAssignableFrom(SlashCommandClass))
            {
                ModularDiscordClient.SlashCommandsExtension.RegisterCommands(SlashCommandClass, guildId);
                if (updateSlashies)
                {
                    ModularDiscordClient.SlashCommandsExtension.RefreshCommands();
                    Console.WriteLine("Slashies registered");
                }
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
            ModularDiscordClient.ModulesActiveGuilds[ModuleName].RemoveAll(x => x == guildId);

            var activeCommandsList = ModularDiscordClient.SlashCommandsExtension.RegisteredCommands;
            var activeCommandsDict = activeCommandsList.ToDictionary(x => x.Key, x => x.Value);

            List<DiscordApplicationCommand> commandList = new();
            if (activeCommandsDict.TryGetValue(guildId, out IReadOnlyList<DiscordApplicationCommand> readonlyCommands))
            {
                commandList = readonlyCommands.ToList();
                commandList.RemoveAll(x => Commands.Contains(x.Name));
            }
            ModularDiscordClient.DiscordClient.BulkOverwriteGuildApplicationCommandsAsync(guildId, commandList);
            if (updateSlashies) ModularDiscordClient.SlashCommandsExtension.RefreshCommands();

            DataProvider.SetConfig(ModularDiscordClient.GuildSettings);
        }
    }

    internal class MadsServiceProvider
    {
        public ModularDiscordBot ModularDiscordBot;

        //-> ModuleName -> List of guildIds where the module is activ
        public Dictionary<string, List<ulong>> ModulesActivGuilds;

        public MadsServiceProvider(ModularDiscordBot modularDiscordBot, Dictionary<string, List<ulong>> modulesActivGuilds)
        {
            this.ModularDiscordBot = modularDiscordBot;
            this.ModulesActivGuilds = modulesActivGuilds;
        }
    }

    /// <summary>
    /// Attribute for modules to check if the module is enabled in given guild
    /// </summary>
    internal class GuildIsEnabled : CheckBaseAttribute
    {
        /// <summary>
        /// Name of the module
        /// </summary>
        private readonly string _moduleName;

        public GuildIsEnabled(string moduleName)
        {
            _moduleName = moduleName;
        }

        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Channel.IsPrivate) return Task.FromResult(true);

            MadsServiceProvider services = (MadsServiceProvider)ctx.CommandsNext.Services.GetService(typeof(MadsServiceProvider));

            if (services.ModulesActivGuilds.TryGetValue(_moduleName, out List<ulong> guilds))
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
            
            if (ctx.Member != null)
            {
                discordEmbedBuilder
                    .WithAuthor(ctx.Member.Nickname, ctx.Member.AvatarUrl, ctx.Member.AvatarUrl);
            }
            
            discordEmbedBuilder
                .WithColor(new DiscordColor(0, 155, 194))
                .WithFooter("Mads")
                .WithTimestamp(DateTime.Now)
                .WithTitle(title)
                .WithDescription(message);
            
            return discordEmbedBuilder;
        }
    }
}