using System.Reflection;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Executors;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using MADS.Commands;
using MADS.Entities;
using MADS.EventListeners;
using MADS.Extensions;
using MADS.JsonModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MADS;

public class ModularDiscordBot
{
    public DiscordClient DiscordClient;
    public LoggingProvider Logging;
    public DateTime StartTime;

    private IDbContextFactory<MadsContext> _dbFactory;
    private ConfigJson _config;
    private ServiceProvider _services;
    private SlashCommandsExtension _slashCommandsExtension;
    private CommandsNextExtension _commandsNextExtension;
    private InteractivityExtension _interactivityExtension;
    
    
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
            Intents = DiscordIntents.All
        };

        DiscordClient = new DiscordClient(discordConfig);
        var connectionString = DataProvider.GetConfig().ConnectionString;

        _services = new ServiceCollection()
                   .AddSingleton(new MadsServiceProvider(this))
                   .AddDbContextFactory<MadsContext>(options =>
                       options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)))
                   .BuildServiceProvider();

        _dbFactory = _services.GetService<IDbContextFactory<MadsContext>>();

        RegisterDSharpExtensions();

        await EnableGuildConfigs();

        DiscordClient.Ready += OnClientReady;
        DiscordClient.Zombied += OnZombied;
        DiscordClient.GuildDownloadCompleted += OnGuildDownloadCompleted;

        DiscordActivity act = new(_config.Prefix + "help", ActivityType.Watching);

        var dbContext = await _dbFactory!.CreateDbContextAsync();

        Console.WriteLine(dbContext.Guilds.ToList().Any());

        //connect client
        try
        {
            await DiscordClient.ConnectAsync(act, UserStatus.Online);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
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

    private async Task EnableGuildConfigs()
    {
        Console.WriteLine("Loading guild configs");

        var dbContext = await _dbFactory.CreateDbContextAsync();
        var defaultGuild = new GuildDbEntity()
        {
            Prefix = "!!",
            DiscordId = 0,
            Incidents = new List<IncidentDbEntity>(),
            Users = new List<GuildUserDbEntity>()
        };

        defaultGuild.Config = new GuildConfigDbEntity()
        {
            DiscordGuildId = defaultGuild.Id,
            Prefix = "!!"
        };

        await dbContext.Guilds.Upsert(defaultGuild)
                       .On(x => x.DiscordId)
                       //.WhenMatched(x => defaultGuild)
                       .RunAsync();
                       
        dbContext.Guilds.Add(defaultGuild);

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

    /// <summary>
    /// Registers all DSharp+ extensions (CNext, SlashCommands, Interactivity), the commands and event handlers for errors
    /// </summary>
    private void RegisterDSharpExtensions()
    {
        var asm = Assembly.GetExecutingAssembly();

        //CNext
        CommandsNextConfiguration cnextConfig = new()
        {
            CaseSensitive = false,
            DmHelp = false,
            EnableDms = true,
            EnableMentionPrefix = true,
            PrefixResolver = GetPrefixPositionAsync,
            Services = _services,
            CommandExecutor = new ParallelQueuedCommandExecutor()
        };
        _commandsNextExtension = DiscordClient.UseCommandsNext(cnextConfig);
        _commandsNextExtension.RegisterCommands(asm);
        _commandsNextExtension.CommandErrored += EventListener.OnCNextErrored;

        //Slashcommands
        SlashCommandsConfiguration slashConfig = new()
        {
            Services = _services
        };
        _slashCommandsExtension = DiscordClient.UseSlashCommands(slashConfig);
        _slashCommandsExtension.RegisterCommands(asm, 938120155974750288);
        _slashCommandsExtension.SlashCommandErrored += EventListener.OnSlashCommandErrored;

        //Custom buttons
        EventListener.EnableButtonListener(DiscordClient);

        //Interactivity
        InteractivityConfiguration interactivityConfig = new()
        {
            PollBehaviour = PollBehaviour.KeepEmojis,
            Timeout = TimeSpan.FromSeconds(600),
            ButtonBehavior = ButtonPaginationBehavior.DeleteButtons,
            PaginationBehaviour = PaginationBehaviour.Ignore,
            AckPaginationButtons = true,
            ResponseBehavior = InteractionResponseBehavior.Ignore,
            ResponseMessage = "invalid interaction",
            PaginationDeletion = PaginationDeletion.DeleteEmojis
        };
        _interactivityExtension = DiscordClient.UseInteractivity(interactivityConfig);
    }

    private static async Task OnZombied(DiscordClient sender, ZombiedEventArgs e)
    {
        await sender.ReconnectAsync(true);
    }

    private static async Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
    {
    }

    public static async Task<DiscordMessage> AnswerWithDelete(CommandContext ctx, DiscordEmbed message,
        int secondsToDelete = 20)
    {
        var response = await ctx.Channel.SendMessageAsync(message);

        if (ctx.Channel.IsPrivate) return response;
        
        await Task.Delay(secondsToDelete * 1000);
        await response.DeleteAsync();
        await ctx.Message.DeleteAsync();

        return response;
    }

    private Task<int> GetPrefixPositionAsync(DiscordMessage msg)
    {
        var dbContext = _dbFactory.CreateDbContext();
        var guildSettings = new GuildDbEntity{ Prefix = "!", Id = 0};

        
        if (msg.Channel.Guild is not null && dbContext.Guilds.Any())
        {
            guildSettings = dbContext.Guilds.FirstOrDefault(x => x.DiscordId == msg.Channel.GuildId) ?? guildSettings;
        }
        else
        {
            guildSettings = dbContext.Guilds.FirstOrDefault(x => x.DiscordId == 0) ?? guildSettings;
        }
        
        
        dbContext.Dispose();
        
        return Task.FromResult(msg.GetStringPrefixLength(guildSettings.Prefix));
    }
}