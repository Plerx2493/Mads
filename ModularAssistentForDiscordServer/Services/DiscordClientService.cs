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
using MADS.Services;
using MADS.JsonModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MADS.Services;

public class DiscordClientService : IHostedService
{
    public DiscordClient DiscordClient;
    public LoggingService Logging;
    public SlashCommandsExtension SlashCommands;
    public CommandsNextExtension CommandsNext;
    public DateTime StartTime;
    private readonly CancellationToken _cancellationToken;
    private readonly ConfigJson _config;
    private IDbContextFactory<MadsContext> _dbContextFactory;
    private IServiceProvider _services;

    public DiscordClientService(ConfigJson pConfig, IServiceProvider services,
        IDbContextFactory<MadsContext> dbDbContextFactory, VolatileMemoryService memoryService)
    {
        Log.Warning("DiscordClientService");

        StartTime = DateTime.Now;
        _services = services;
        _config = pConfig;
        _dbContextFactory = dbDbContextFactory;
        Logging = new LoggingService(this);

        DiscordConfiguration discordConfig = new()
        {
            Token = _config.Token,
            TokenType = TokenType.Bot,
            AutoReconnect = true,
            MinimumLogLevel = _config.LogLevel,
            LoggerFactory = new LoggerFactory().AddSerilog(),
            Intents = DiscordIntents.All
        };

        DiscordClient = new DiscordClient(discordConfig);

        EventListener.GuildDownload(DiscordClient, _dbContextFactory);
        EventListener.EnableMessageSniper(DiscordClient, memoryService);
        EventListener.AddGuildNotifier(DiscordClient, Logging);
        EventListener.VoiceTrollListener(DiscordClient, memoryService);

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
        CommandsNext = DiscordClient.UseCommandsNext(cnextConfig);
        CommandsNext.RegisterCommands(asm);
        CommandsNext.CommandErrored += EventListener.OnCNextErrored;

        //Slashcommands
        SlashCommandsConfiguration slashConfig = new()
        {
            Services = _services
        };
        SlashCommands = DiscordClient.UseSlashCommands(slashConfig);
#if RELEASE
        SlashCommands.RegisterCommands(asm);
#else
        DiscordClient.Logger.LogWarning("DEBUG");
        SlashCommands.RegisterCommands(asm, 938120155974750288);
#endif
        SlashCommands.SlashCommandErrored += EventListener.OnSlashCommandErrored;

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
        DiscordClient.UseInteractivity(interactivityConfig);

        DiscordClient.Zombied += EventListener.OnZombied;
        DiscordClient.GuildDownloadCompleted += OnGuildDownloadCompleted;
        DiscordClient.MessageCreated += EventListener.DmHandler;
        DiscordClient.ClientErrored += EventListener.OnClientErrored;
    }

    private Task OnGuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
    {
        Logging.Setup();
        return Task.CompletedTask;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        //Update database to latest version
        var context = await _dbContextFactory.CreateDbContextAsync(_cancellationToken);
        if ((await context.Database.GetPendingMigrationsAsync()).Any())
            await context.Database.MigrateAsync(_cancellationToken);

        DiscordActivity act = new("over some Servers", ActivityType.Watching);


        //connect client
        await DiscordClient.ConnectAsync(act, UserStatus.Online);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return DiscordClient.DisconnectAsync();
    }
}