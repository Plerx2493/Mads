using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MADS.Extensions;
using MADS.JsonModel;
using MADS.Modules;
using DSharpPlus.EventArgs;
using ModularAssistentForDiscordServer.Utility;
using DSharpPlus.SlashCommands.EventArgs;

namespace MADS
{
    internal class ModularDiscordBot
    {
        internal DiscordClient DiscordClient;
        internal LoggingProvider Logging;
        internal CommandsNextExtension CommandsNextExtension;
        internal SlashCommandsExtension SlashCommandsExtension;
        internal Dictionary<string, IMadsModul> madsModules;
        internal Dictionary<string, List<ulong>> ModulesAktivGuilds;
        private Dictionary<ulong, GuildSettings> _Guildsettings;
        internal Dictionary<ulong, GuildSettings> GuildSettings
        {
            get
            {
                return _Guildsettings;
            }
            set
            {
                _Guildsettings = value;
                DataProvider.SetConfig(value);
            }
        }
        internal ServiceProvider Services;
        internal DateTime startTime;
        internal ConfigJson config;


        public ModularDiscordBot()
        {
            madsModules = new Dictionary<string, IMadsModul>();
            ModulesAktivGuilds = new Dictionary<string, List<ulong>>();
            startTime = DateTime.Now;
            Logging = new LoggingProvider(this);
        }

        public async Task RunAsync()
        {
            if (Innit()) return;

            RegisterModul(typeof(ModerationModul));

            config = DataProvider.GetConfig();

            GuildSettings = config.GuildSettings;
            
            DiscordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = config.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = config.LogLevel,
                Intents = GetRequiredIntents()
            });

            Services = new ServiceCollection()
                .AddSingleton(new MadsServiceProvider(this, ModulesAktivGuilds))
                .BuildServiceProvider();

            RegisterCommandExtensions();

            EnableGuildConfigs();

            DiscordClient.Ready += OnClientReady;
            DiscordClient.Zombied += OnZombied;


            //connect client
            await DiscordClient.ConnectAsync();
            //keep alive
            await Task.Delay(-1);
        }

        private void EnableGuildConfigs()
        {
            config.GuildSettings.ToList().ForEach(x =>
            {
                x.Value.AktivModules.ForEach(y =>
                {
                    if (madsModules.TryGetValue(y, out IMadsModul madsModul))
                    {
                        Console.WriteLine("Enabled modul: " + x.Key + ":" + madsModul.ModulName);
                        madsModul.RegisterCommands(x.Key, false);
                    }
                });
            });

            Console.WriteLine("EnableGuildConfigs() finished");
        }

        public static bool Innit()
        {
            string configPath = DataProvider.GetPath("config.json");

            if (File.Exists(configPath)) { return false; }

            var tmp = File.Create(configPath);
            tmp.Close();

            ConfigJson newConfig = new()
            {
                Token = "<Your Token here>",
                Prefix = "!",
                LogLevel = LogLevel.Debug,
                DiscordEmbed = new DiscordEmbedBuilder()
                {
                    Color = new(new(0, 255, 194)),
                    Footer = new()
                    {
                        Text = "Mads"
                    }
                },
                GuildSettings = new Dictionary<ulong, GuildSettings>()
            };

            newConfig.GuildSettings[0] = new();
            JsonProvider.parseJson(configPath, newConfig);

            Console.WriteLine("Please insert your token in the config file and Restart");
            Console.WriteLine("Filepath: " + configPath);
            Console.WriteLine("Press key to continue");
            Console.Read();

            return true;
        }

        private void RegisterCommandExtensions()
        {
            CommandsNextConfiguration comandsConfig = new()
            {
                CaseSensitive = false,
                DmHelp = false,
                EnableDms = true,
                EnableMentionPrefix = true,
                PrefixResolver = GetPrefixPositionAsync,
                Services = Services
            };

            CommandsNextExtension = DiscordClient.UseCommandsNext(comandsConfig);
            CommandsNextExtension.RegisterCommands<BaseCommands>();

            madsModules.ToList().ForEach(x => x.Value.RegisterCNext());

            SlashCommandsConfiguration slashConfig = new()
            {
                Services = Services
            };

            SlashCommandsExtension = DiscordClient.UseSlashCommands(slashConfig);

            CommandsNextExtension.CommandErrored += OnCNextErrored;
            SlashCommandsExtension.SlashCommandErrored += OnSlashCommandErrored;
        }

        private async Task OnSlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
            var typeOfException = e.Exception.GetType();
            if (typeOfException == typeof(SlashExecutionChecksFailedException) || typeOfException == typeof(ArgumentException))
            {
                return;
            }

            var DiscordEmbed = new DiscordEmbedBuilder
            {
                Title = "Error",
                Description = $"The command execution failed",
                Color = DiscordColor.Red,
                Timestamp = DateTime.Now,
            };
            DiscordEmbed.AddField("Exception:", e.Exception.Message + "\n" + e.Exception.StackTrace);

            await e.Context.Interaction.CreateResponseAsync( InteractionResponseType.ChannelMessageWithSource ,new DiscordInteractionResponseBuilder().AddEmbed(DiscordEmbed));
        }

        private async Task OnCNextErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            var typeOfException = e.Exception.GetType();
            if (typeOfException == typeof(ChecksFailedException) || typeOfException == typeof(ArgumentException))
            {
                return;
            }

            await e.Context.Message.RespondAsync($"OOPS your command just errored... \n {e.Exception}");
        }

        private async Task OnZombied(DiscordClient sender, ZombiedEventArgs e)
        {
            await DiscordClient.ReconnectAsync(true);
        }

        private async Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            DiscordActivity act = new(config.Prefix + "help", ActivityType.Watching);
            await DiscordClient.UpdateStatusAsync(act, UserStatus.Online);
        }

        public void RegisterModul(Type modul)
        {
            var newModul = (IMadsModul)Activator.CreateInstance(modul, this);
            madsModules[newModul.ModulName] = newModul;
        }

        public static async Task<DiscordMessage> AnswerWithDelete(CommandContext ctx, DiscordEmbed message, int secondsToDelete = 20)
        {
            DiscordMessage response = await ctx.Channel.SendMessageAsync(message);

            if (!ctx.Channel.IsPrivate)
            {
                await Task.Delay(secondsToDelete * 1000);
                await response.DeleteAsync();
                await ctx.Message.DeleteAsync();
            }

            return response;
        }

        public DiscordIntents GetRequiredIntents()
        {
            DiscordIntents requiredIntents =
                DiscordIntents.GuildMessages
                | DiscordIntents.DirectMessages
                | DiscordIntents.Guilds;

            madsModules.ToList().ForEach(x =>
            {
                requiredIntents |= x.Value.RequiredIntents;
            });

            return requiredIntents;
        }

        public Task<int> GetPrefixPositionAsync(DiscordMessage msg)
        {
            GuildSettings guildSettings;
            var allGuildSettings = GuildSettings;

            if (msg.Channel.Guild is not null)
            {
                if (!allGuildSettings.TryGetValue(msg.Channel.Guild.Id, out guildSettings))
                {
                    guildSettings = allGuildSettings[0];
                }
            }
            else
            {
                guildSettings = allGuildSettings[0];
            }

            if (guildSettings.Prefix == null) guildSettings.Prefix = allGuildSettings[0].Prefix;

            return Task.FromResult(msg.GetStringPrefixLength(guildSettings.Prefix));
        }
    }
}