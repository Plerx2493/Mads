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
using MADS.EventListeners;
using MADS.Extensions;
using MADS.JsonModel;
using MADS.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MADS;

public class ModularDiscordBot : IDisposable
{
    public readonly LoggingProvider       Logging;
    private         CancellationToken     _cancellationToken;
    private         CommandsNextExtension _commandsNextExtension;

    private ConfigJson             _config;
    private InteractivityExtension _interactivityExtension;
    private ServiceProvider        _services;
    private SlashCommandsExtension _slashCommandsExtension;
    private TokenListener          _tokenListener;
    public  DiscordClient          DiscordClient;
    public  DateTime               StartTime;


    public ModularDiscordBot()
    {
        StartTime = DateTime.Now;
        Logging = new LoggingProvider(this);
    }

    public void Dispose()
    {
        _commandsNextExtension?.Dispose();
        _services?.Dispose();
        DiscordClient?.Dispose();
        _tokenListener.Dispose();
    }

    public async Task<bool> RunAsync(ConfigJson pConfig, CancellationToken token)
    {
        _cancellationToken = token;
        _config = pConfig;

        DiscordConfiguration discordConfig = new()
        {
            Token = _config.Token,
            TokenType = TokenType.Bot,
            AutoReconnect = true,
            MinimumLogLevel = _config.LogLevel,
            Intents = DiscordIntents.All
        };

        DiscordClient = new DiscordClient(discordConfig);
        _tokenListener = new TokenListener("51151", "/api/v1/mads/token/");

#pragma warning disable CS4014
        _tokenListener.StartAsync(_cancellationToken);
#pragma warning restore CS4014

        _services = new ServiceCollection()
                    .AddSingleton(this)
                    .AddMemoryCache(options => options.ExpirationScanFrequency = TimeSpan.FromMinutes(10))
                    .AddSingleton<VolatileMemoryService>()
                    .AddSingleton(_tokenListener)
                    .BuildServiceProvider();


        RegisterDSharpExtensions();

        EventListener.EnableMessageSniper(DiscordClient, _services.GetService<VolatileMemoryService>());
        await EventListener.VoiceTrollListener(DiscordClient, _services.GetService<VolatileMemoryService>());
        DiscordClient.Zombied += EventListener.OnZombied;
        DiscordClient.GuildDownloadCompleted += OnGuildDownloadCompleted;
        DiscordClient.MessageCreated += EventListener.DmHandler;

        DiscordActivity act = new("over some Servers", ActivityType.Watching);


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
        await Task.Delay(-1, token);
        //
        //DEADZONE
        //
        return true;
    }

    private Task OnGuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
    {
        Logging.Setup();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Registers all DSharp+ extensions (CNext, SlashCommands, Interactivity), the commands and event handlers for errors
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
#if RELEASE
        _slashCommandsExtension.RegisterCommands(asm);
#else
        _slashCommandsExtension.RegisterCommands(asm, 938120155974750288);
#endif
        _slashCommandsExtension.SlashCommandErrored += EventListener.OnSlashCommandErrored;

        //Custom buttons
        EventListener.EnableButtonListener(DiscordClient);
        EventListener.EnableRoleSelectionListener(DiscordClient);

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


    public static async Task<DiscordMessage> AnswerWithDelete
    (
        CommandContext ctx,
        DiscordEmbed message,
        int secondsToDelete = 20
    )
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
        return Task.FromResult(-1);
    }
}