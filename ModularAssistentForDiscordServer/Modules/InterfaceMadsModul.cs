using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext.Attributes;

namespace MADS.Modules
{
    internal interface IMadsModul
    {
        ModularDiscordBot ModularDiscordClient { get; set; }
        string ModulName { get; set; }
        string ModulDescription { get; set; }
        string[] Commands { get; set; }
        Dictionary<string, string> CommandDescriptions { get; set; }
        Type? CommandClass { get; set; }
        Type? SlashCommandClass { get; set; }
        DiscordIntents RequiredIntents { get; set; }

        public Task ModulMainTask();

        public void Enable(ulong guildId)
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
                ModularDiscordClient.SlashCommandsExtension.RegisterCommands(SlashCommandClass, guildId);
            }
        }
    }
    
    internal class MadsServiceProvider
    {
        public ModularDiscordBot modularDiscordBot;
        public Dictionary<string, List<ulong>> modulesActivGuilds;

        public MadsServiceProvider(ModularDiscordBot modularDiscordBot, Dictionary<string, List<ulong>> modulesActivGuilds)
        {
            this.modularDiscordBot = modularDiscordBot;
            this.modulesActivGuilds = modulesActivGuilds;
        }
    }

    internal class GuildIsEnabled : CheckBaseAttribute
    {
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

    internal interface IMadsCommandBase
    {
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
