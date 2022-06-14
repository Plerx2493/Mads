﻿using DSharpPlus;
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
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        Type? CommandClass { get; set; }
        Type? SlashCommandClass { get; set; }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        DiscordIntents RequiredIntents { get; set; }

        public void RegisterCNext();

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
        }
        
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