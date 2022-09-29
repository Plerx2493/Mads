using System.Reflection;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MADS;

public class ModularDiscordBot
{
    public DiscordClient   DiscordClient;
    public LoggingProvider Logging;
    public DateTime        StartTime;
    public MadsContext     Data; 
    
    private IDbContextFactory<MadsContext> _dbFactory;
    private ConfigJson                     _config;
    private ServiceProvider                _services;
    private SlashCommandsExtension         _slashCommandsExtension;
    private CommandsNextExtension          _commandsNextExtension;
    


    public ModularDiscordBot()
    {
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

        _config = DataProvider.GetConfig();
        
        DiscordConfiguration discordConfig = new()
        {
            Token = _config.Token,
            TokenType = TokenType.Bot,
            AutoReconnect = true,
            MinimumLogLevel = _config.LogLevel,
            Intents = GetRequiredIntents()
        };

        DiscordClient = new DiscordClient(discordConfig);
        var connectionString = DataProvider.GetConfig().ConnectionString;
        
        _services = new ServiceCollection()
                   .AddSingleton(new MadsServiceProvider(this))
                   .AddDbContextFactory<MadsContext>(options =>
                       options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)))
                   .BuildServiceProvider();

        _dbFactory = _services.GetService<IDbContextFactory<MadsContext>>();
        
        RegisterCommandExtensions();

        EnableGuildConfigs();

        DiscordClient.Ready += OnClientReady;
        DiscordClient.Zombied += OnZombied;
        DiscordClient.GuildDownloadCompleted += OnGuildDownloadCompleted;

        DiscordActivity act = new(_config.Prefix + "help", ActivityType.Watching);

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

    private async void EnableGuildConfigs()
    {
        Console.WriteLine("Loading guild configs");

        var dbContext = await _dbFactory.CreateDbContextAsync();
        var defaultGuild = new GuildDbEntity()
        {
            Id = 0,
            Prefix = "!",
            Incidents = new(),
            Users = new()
        };
        
        defaultGuild.Config = new GuildConfigDbEntity()
        {
            Guild = defaultGuild,
            GuildId = defaultGuild.Id,
            Prefix = "!"
        };
        
        dbContext.Guilds.Upsert(defaultGuild);
        await dbContext.SaveChangesAsync();
        Console.WriteLine("Guild configs loaded");
    }

    private static bool ValidateConfig()
    {
        var configPath = DataProvider.GetPath("config.json");

        if (!File.Exists(configPath)) { return false; }

        var lConfig = DataProvider.GetConfig();

        if (lConfig.Token is null or "" or "<Your Token here>") { return false; }
        if (lConfig.Prefix is null or "") { lConfig.Prefix = "!"; }
        if (lConfig.ConnectionString is null or "") return false;

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
        };
        JsonProvider.ParseJson(configPath, newConfig);

        Console.WriteLine("Please insert your token in the config file and restart");
        Console.WriteLine("Filepath: " + configPath);
        Console.WriteLine("Press key to continue");
        Console.Read();
    }

    private void RegisterCommandExtensions()
    {
        var asm = Assembly.GetExecutingAssembly();
        
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

        _commandsNextExtension = DiscordClient.UseCommandsNext(commandsConfig);
        _commandsNextExtension.RegisterCommands(asm);
        
        SlashCommandsConfiguration slashConfig = new()
        {
            Services = _services
        };

        _slashCommandsExtension = DiscordClient.UseSlashCommands(slashConfig);
        _slashCommandsExtension.RegisterCommands(asm);
        _commandsNextExtension.CommandErrored += OnCNextErrored;
        _slashCommandsExtension.SlashCommandErrored += OnSlashCommandErrored;

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

    private static DiscordIntents GetRequiredIntents()
    {
        const DiscordIntents requiredIntents = DiscordIntents.All;
        return requiredIntents;
    }

    private Task<int> GetPrefixPositionAsync(DiscordMessage msg)
    {
        var dbContext = _dbFactory.CreateDbContext();
        GuildDbEntity guildSettings = new GuildDbEntity();
        
        if (msg.Channel.Guild is not null)
        {
            guildSettings = dbContext.Guilds.FirstOrDefault(x => x.Id == msg.Channel.GuildId);
        }
        else
        {
            guildSettings = dbContext.Guilds.FirstOrDefault(x => x.Id == 0);
        }

        return Task.FromResult(msg.GetStringPrefixLength(guildSettings.Prefix));
    }
}