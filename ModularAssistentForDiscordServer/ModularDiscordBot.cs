// Copyright 2023 Plerx2493
//
// Licensed under the Apache License, Version 2.0 (the "License")
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
using MADS.Entities;
using MADS.EventListeners;
using MADS.Extensions;
using MADS.JsonModel;
using MADS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Serilog;
using Serilog.Extensions.Logging;

namespace MADS;

public class ModularDiscordBot : IDisposable
{
    public readonly LoggingProvider Logging;
    private CancellationToken _cancellationToken;
    private CommandsNextExtension _commandsNextExtension;

    private ConfigJson _config;
    private InteractivityExtension _interactivityExtension;
    private ServiceProvider _services;
    private SlashCommandsExtension _slashCommandsExtension;
    public DiscordClient DiscordClient;
    public DiscordRestClient DiscordRestClient;
    public DateTime StartTime;
    private IDbContextFactory<MadsContext> _contextProvider;


    public ModularDiscordBot()
    {
        StartTime = DateTime.Now;
        Logging = new LoggingProvider(this);
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        try
        {
            _services?.Dispose();
        }
        catch (Exception)
        {
            // ignored
        }

        try
        {
            DiscordClient?.Dispose();
        }
        catch (Exception)
        {
            // ignored
        }
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
            Intents = DiscordIntents.All,
            LoggerFactory = new LoggerFactory().AddSerilog()
        };

        DiscordClient = new DiscordClient(discordConfig);
        
        var config = DataProvider.GetConfig();

        var discordRestConfig = new DiscordConfiguration
        {
            Token = config.Token
        };

        DiscordRestClient = new DiscordRestClient(discordRestConfig);

        _services = new ServiceCollection()
            .AddSingleton(Log.Logger)
            .AddSingleton(this)
            .AddQuartz(x =>
            {
                x.UsePersistentStore(options =>
                {
                    options.UseMySql(opt =>
                    {
                        opt.ConnectionString = config.ConnectionString;
                        opt.TablePrefix = "QRTZ_";
                    });
                });

                x.InterruptJobsOnShutdownWithWait = true;
                x.UseMicrosoftDependencyInjectionJobFactory();
                x.UseSimpleTypeLoader();

                x.SchedulerName = "reminder-scheduler";
            })
            .AddSingleton(DiscordClient)
            .AddSingleton(DiscordRestClient)
            .AddDbFactoryDebugOrRelease(_config)
            .AddMemoryCache(options =>
            {
                options.ExpirationScanFrequency = TimeSpan.FromMinutes(10);
                options.SizeLimit = 1024L;
            })
            .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ModularDiscordBot).Assembly))
            .AddSingleton<VolatileMemoryService>()
            .AddSingleton<QuotesService>()
            .AddSingleton<StarboardService>()
            .AddHostedService(s => s.GetRequiredService<StarboardService>())
            .AddSingleton(s => new TokenListener("51151", s.GetRequiredService<DiscordClient>(), "/api/v1/mads/token/"))
            .AddHostedService(s => s.GetRequiredService<TokenListener>())
            .AddSingleton<ReminderService>()
            .AddHostedService(s => s.GetRequiredService<ReminderService>())
            .AddQuartzHostedService(options =>
            {
                // when shutting down we want jobs to complete gracefully
                options.WaitForJobsToComplete = true;
            })
            .BuildServiceProvider();

        RegisterDSharpExtensions();


        //Update database to latest version
        _contextProvider = _services.GetService<IDbContextFactory<MadsContext>>();
        var context = await _contextProvider.CreateDbContextAsync(_cancellationToken);
        if ((await context.Database.GetPendingMigrationsAsync(token)).Any())
        {
            await context.Database.MigrateAsync(_cancellationToken);
        }

        EventListener.GuildDownload(DiscordClient, _contextProvider);
        EventListener.EnableMessageSniper(DiscordClient, _services.GetService<VolatileMemoryService>());
        EventListener.AddGuildNotifier(this);
        await EventListener.VoiceTrollListener(DiscordClient, _services.GetService<VolatileMemoryService>());

        //Make sure hosted services are running
#pragma warning disable CS4014
        _services.GetRequiredService<StarboardService>().StartAsync(_cancellationToken);
        _services.GetRequiredService<TokenListener>().StartAsync(_cancellationToken);
        _services.GetRequiredService<ReminderService>().StartAsync(_cancellationToken);
#pragma warning restore CS4014

        DiscordClient.Zombied += EventListener.OnZombied;
        DiscordClient.GuildDownloadCompleted += OnGuildDownloadCompleted;
        DiscordClient.MessageCreated += EventListener.DmHandler;
        DiscordClient.ClientErrored += EventListener.OnClientErrored;

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
        DiscordClient.Logger.LogWarning("DEBUG");
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
            Timeout = TimeSpan.FromMinutes(10),
            ButtonBehavior = ButtonPaginationBehavior.DeleteButtons,
            PaginationBehaviour = PaginationBehaviour.Ignore,
            ResponseBehavior = InteractionResponseBehavior.Ignore,
            ResponseMessage = "invalid interaction",
            PaginationDeletion = PaginationDeletion.DeleteEmojis
        };
        _interactivityExtension = DiscordClient.UseInteractivity(interactivityConfig);
    }
}