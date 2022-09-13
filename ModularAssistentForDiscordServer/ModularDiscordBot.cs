using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.CommandsNext.Executors;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using MADS.Commands;
using MADS.Commands.Text;
using MADS.CustomComponents;
using MADS.Entities;
using MADS.Extensions;
using MADS.JsonModel;
using MADS.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MADS;

public class ModularDiscordBot
{
    
    internal ConfigJson             config;
    public   DiscordClient          DiscordClient;
    public   LoggingProvider        Logging;
    private  ServiceProvider        Services;
    public   SlashCommandsExtension SlashCommandsExtension;
    public   CommandsNextExtension  CommandsNextExtension;
    internal DateTime               startTime;


    public ModularDiscordBot()
    {
        startTime = DateTime.Now;
        Logging = new LoggingProvider(this);
    }

    public async Task RunAsync()
    {
        if (!ValidateConfig())
        {
            CreateConfig();
            return;
        }

        config = DataProvider.GetConfig();
        
        DiscordConfiguration discordConfig = new()
        {
            Token = config.Token,
            TokenType = TokenType.Bot,
            AutoReconnect = true,
            MinimumLogLevel = config.LogLevel,
            Intents = GetRequiredIntents()
        };

        DiscordClient = new DiscordClient(discordConfig);

        Services = new ServiceCollection()
                   .AddSingleton(new MadsServiceProvider(this))
                   .BuildServiceProvider();

        RegisterCommandExtensions();

        EnableGuildConfigs();

        DiscordClient.Ready += OnClientReady;
        DiscordClient.Zombied += OnZombied;
        DiscordClient.GuildDownloadCompleted += OnGuildDownloadCompleted;

        DiscordActivity act = new(config.Prefix + "help", ActivityType.Watching);

        //connect client
        await DiscordClient.ConnectAsync(act, UserStatus.Online);
        //keep alive
        await Task.Delay(-1);
        //
        //DEADZONE
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

        config.GuildSettings.ToList().ForEach(x =>
        {
            x.Value.AktivModules.ForEach(y =>
            {
                
            });
        });

        SlashCommandsExtension.RefreshCommands();

        Console.WriteLine("Guild configs loaded");
    }

    private static bool ValidateConfig()
    {
        var configPath = DataProvider.GetPath("config.json");

        if (!File.Exists(configPath)) { return false; }

        var lConfig = DataProvider.GetConfig();

        if (lConfig.Token is null or "" or "<Your Token here>") { return false; }
        if (lConfig.Prefix is null or "") { lConfig.Prefix = "!"; }

        lConfig.GuildSettings ??= new Dictionary<ulong, GuildSettings>
        {
            [0] = new()
        };

        var guildSettings = lConfig.GuildSettings;
        Dictionary<ulong, GuildSettings> newGuildSettings = new();

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
        var configPath = DataProvider.GetPath("config.json");

        var fileStream = File.Create(configPath);
        fileStream.Close();

        ConfigJson newConfig = new()
        {
            Token = "<Your Token here>",
            Prefix = "!",
            LogLevel = LogLevel.Debug,
            DiscordEmbed = new DiscordEmbedBuilder
            {
                Color = new Optional<DiscordColor>(new DiscordColor(0, 255, 194)),
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "Mads"
                }
            },
            GuildSettings = new Dictionary<ulong, GuildSettings>()
        };

        newConfig.GuildSettings[0] = new GuildSettings();
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
            Services = Services,
            CommandExecutor = new ParallelQueuedCommandExecutor()
        };

        CommandsNextExtension = DiscordClient.UseCommandsNext(commandsConfig);
        CommandsNextExtension.RegisterCommands<BaseCommands>();
        CommandsNextExtension.RegisterCommands<ModerationCommands>();
        CommandsNextExtension.RegisterCommands<DevCommands>();
        
        SlashCommandsConfiguration slashConfig = new()
        {
            Services = Services
        };

        SlashCommandsExtension = DiscordClient.UseSlashCommands(slashConfig);

        CommandsNextExtension.CommandErrored += OnCNextErrored;
        SlashCommandsExtension.SlashCommandErrored += OnSlashCommandErrored;

        ActionDiscordButton.EnableButtonListener(DiscordClient);
    }

    private async Task OnSlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
    {
        var typeOfException = e.Exception.GetType();
        if (typeOfException == typeof(SlashExecutionChecksFailedException)
            || typeOfException == typeof(ArgumentException))
        {
            return;
        }

        DiscordEmbedBuilder discordEmbed = new()
        {
            Title = "Error",
            Description = "The command execution failed",
            Color = DiscordColor.Red,
            Timestamp = DateTime.Now
        };
        discordEmbed.AddField("Exception:", e.Exception.Message + "\n" + e.Exception.StackTrace);

        await e.Context.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AddEmbed(discordEmbed));
    }

    private async Task OnCNextErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
    {
        var typeOfException = e.Exception.GetType();
        if (typeOfException == typeof(ChecksFailedException) || typeOfException == typeof(ArgumentException)
                                                             || typeOfException == typeof(CommandNotFoundException))
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
    }

    public static async Task<DiscordMessage> AnswerWithDelete(CommandContext ctx, DiscordEmbed message,
        int secondsToDelete = 20)
    {
        var response = await ctx.Channel.SendMessageAsync(message);

        if (!ctx.Channel.IsPrivate)
        {
            await Task.Delay(secondsToDelete * 1000);
            await response.DeleteAsync();
            await ctx.Message.DeleteAsync();
        }

        return response;
    }

    public static DiscordIntents GetRequiredIntents()
    {
        var requiredIntents = DiscordIntents.All;
            //DiscordIntents.GuildMessages
            //| DiscordIntents.DirectMessages
            //| DiscordIntents.Guilds
            //| DiscordIntents.GuildVoiceStates;

        

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