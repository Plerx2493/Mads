using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.CommandsNext.Executors;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using MADS.Entities;
using MADS.Extensions;
using MADS.JsonModel;
using MADS.Modules;
using MADS.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MADS
{
    public class ModularDiscordBot
    {
        public DiscordClient DiscordClient;
        public LoggingProvider Logging;
        public CommandsNextExtension CommandsNextExtension;
        public SlashCommandsExtension SlashCommandsExtension;

        //ModuleName -> Module instance
        public Dictionary<string, IMadsModul> MadsModules;

        //ModuleName -> Guild Ids which have enabled the module
        public readonly Dictionary<string, List<ulong>> ModulesActiveGuilds;

        //GuildId -> Guild settings for certain guild
        public Dictionary<ulong, GuildSettings> GuildSettings;

        private ServiceProvider _services;
        internal DateTime StartTime;
        internal ConfigJson Config;

        public ModularDiscordBot()
        {
            MadsModules = new Dictionary<string, IMadsModul>();
            ModulesActiveGuilds = new Dictionary<string, List<ulong>>();
            StartTime = DateTime.Now;
            Logging = new LoggingProvider(this);
        }

        public async Task RunAsync()
        {
            if (!ValidateConfig())
            {
                CreateConfig();
                return;
            }

            RegisterModule(typeof(ModerationModule));
            RegisterModule(typeof(DevModule));

            Config = DataProvider.GetConfig();

            GuildSettings = Config.GuildSettings;
            var discordConfig = new DiscordConfiguration
            {
                Token = Config.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = Config.LogLevel,
                Intents = GetRequiredIntents()
            };

            DiscordClient = new DiscordClient(discordConfig);

            _services = new ServiceCollection()
                .AddSingleton(new MadsServiceProvider(this, ModulesActiveGuilds))
                .BuildServiceProvider();

            RegisterCommandExtensions();

            EnableGuildConfigs();

            DiscordClient.Ready += OnClientReady;
            DiscordClient.Zombied += OnZombied;
            DiscordClient.GuildDownloadCompleted += OnGuildDownloadCompleted;

            DiscordActivity act = new(Config.Prefix + "help", ActivityType.Watching);

            //connect client
            await DiscordClient.ConnectAsync(act, UserStatus.Online);
            //keep alive
            await Task.Delay(-1);
            //
            //DEAD ZONE
            //
        }

        private Task OnGuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
        {
            Logging.Setup();
            return Task.CompletedTask;
        }

        private void EnableGuildConfigs()
        {
            Console.WriteLine("Loading guild configs");

            Config.GuildSettings.ToList().ForEach(x =>
            {
                x.Value.AktivModules.ForEach(y =>
                {
                    if (!MadsModules.TryGetValue(y, out IMadsModul madsModule))
                    {
                        return;
                    }

                    Console.WriteLine("module:" + x.Key + ":" + madsModule.ModuleName);
                    madsModule.RegisterCommands(x.Key, false);
                });
            });

            SlashCommandsExtension.RefreshCommands();

            Console.WriteLine("Guild configs loaded");
        }

        private static bool ValidateConfig()
        {
            string configPath = DataProvider.GetPath("config.json");

            if (!File.Exists(configPath)) { return false; }

            ConfigJson lConfig = DataProvider.GetConfig();

            if (lConfig.Token is null or "" or "<Your Token here>") { return false; }
            if (lConfig.Prefix is null or "") { lConfig.Prefix = "!"; }

            lConfig.GuildSettings ??= new Dictionary<ulong, GuildSettings>
            {
                [0] = new()
            };

            var guildSettings = lConfig.GuildSettings;
            var newGuildSettings = new Dictionary<ulong, GuildSettings>();

            foreach (var guild in guildSettings)
            {
                var settings = guild.Value;
                settings.AktivModules = guild.Value.AktivModules.Distinct().ToList();
                newGuildSettings[guild.Key] = settings;
            }

            lConfig.GuildSettings = newGuildSettings;
            DataProvider.SetConfig(lConfig);

            return true;
        }

        private static void CreateConfig()
        {
            string configPath = DataProvider.GetPath("config.json");

            FileStream fileStream = File.Create(configPath);
            fileStream.Close();

            ConfigJson newConfig = new()
            {
                Token = "<Your Token here>",
                Prefix = "!",
                LogLevel = LogLevel.Debug,
                DiscordEmbed = new DiscordEmbedBuilder()
                {
                    Color = new Optional<DiscordColor>(new DiscordColor(0, 255, 194)),
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = "Mads"
                    }
                },
                GuildSettings = new Dictionary<ulong, GuildSettings>
                {
                    [0] = new()
                }
            };

            JsonProvider.ParseJson(configPath, newConfig);

            Console.WriteLine("Please insert your token in the config file and restart");
            Console.WriteLine("Filepath: " + configPath);
            Console.WriteLine("Press key to continue");
            Console.Read();
        }

        private void RegisterCommandExtensions()
        {
            CommandsNextConfiguration commandsConfig = new()
            {
                CaseSensitive = false,
                DmHelp = false,
                EnableDms = true,
                EnableMentionPrefix = true,
                PrefixResolver = GetPrefixPositionAsync,
                Services = _services,
                CommandExecutor = new ParallelQueuedCommandExecutor()
            };

            CommandsNextExtension = DiscordClient.UseCommandsNext(commandsConfig);
            CommandsNextExtension.RegisterCommands<BaseCommands>();

            MadsModules.ToList().ForEach(x => x.Value.RegisterCNext());

            SlashCommandsConfiguration slashConfig = new()
            {
                Services = _services
            };

            SlashCommandsExtension = DiscordClient.UseSlashCommands(slashConfig);

            CommandsNextExtension.CommandErrored += OnCNextErrored;
            SlashCommandsExtension.SlashCommandErrored += OnSlashCommandErrored;

            ActionDiscordButton.EnableButtonListener(DiscordClient);
        }

        private static async Task OnSlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
            var typeOfException = e.Exception.GetType();
            if (typeOfException == typeof(SlashExecutionChecksFailedException) || typeOfException == typeof(ArgumentException))
            {
                return;
            }

            var discordEmbed = new DiscordEmbedBuilder
            {
                Title = "Error",
                Description = $"The command execution failed",
                Color = DiscordColor.Red,
                Timestamp = DateTime.Now,
            };
            discordEmbed.AddField("Exception:", e.Exception.Message + "\n" + e.Exception.StackTrace);

            await e.Context.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(discordEmbed));
        }

        private static async Task OnCNextErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            var typeOfException = e.Exception.GetType();
            if (typeOfException == typeof(ChecksFailedException) || typeOfException == typeof(ArgumentException) || typeOfException == typeof(CommandNotFoundException))
            {
                return;
            }

            await e.Context.Message.RespondAsync($"OOPS your command just errored... \n {e.Exception.Message}");
        }

        private async Task OnZombied(DiscordClient sender, ZombiedEventArgs e)
        {
            await DiscordClient.ReconnectAsync(true);
        }

        private async Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            var discordConfig = new DiscordConfiguration
            {
                Token = Config.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = Config.LogLevel,
                Intents = GetRequiredIntents()
            };

            DiscordRestClient tmp = new(discordConfig);

            //TODO: RestClient test
        }

        private void RegisterModule(Type module)
        {
            IMadsModul newModule = (IMadsModul)Activator.CreateInstance(module, this);
            if (newModule != null)
            {
                MadsModules[newModule.ModuleName] = newModule;
            }
        }

        public static async Task<DiscordMessage> AnswerWithDelete(CommandContext ctx, DiscordEmbed message, int secondsToDelete = 20)
        {
            DiscordMessage response = await ctx.Channel.SendMessageAsync(message);

            if (ctx.Channel.IsPrivate)
            {
                return response;
            }

            await Task.Delay(secondsToDelete * 1000);
            await response.DeleteAsync();
            await ctx.Message.DeleteAsync();

            return response;
        }

        private DiscordIntents GetRequiredIntents()
        {
            DiscordIntents requiredIntents =
                DiscordIntents.GuildMessages
                | DiscordIntents.DirectMessages
                | DiscordIntents.Guilds;

            MadsModules.ToList().ForEach(x =>
            {
                requiredIntents |= x.Value.RequiredIntents;
            });

            return requiredIntents;
        }

        private Task<int> GetPrefixPositionAsync(DiscordMessage msg)
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

            guildSettings.Prefix ??= allGuildSettings[0].Prefix;

            return Task.FromResult(msg.GetStringPrefixLength(guildSettings.Prefix));
        }
    }
}