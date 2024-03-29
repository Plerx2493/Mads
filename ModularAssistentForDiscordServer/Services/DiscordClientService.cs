﻿// Copyright 2023 Plerx2493
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


using System.Diagnostics;
using System.Reflection;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using MADS.Entities;
using MADS.EventListeners;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Serilog.ILogger;

namespace MADS.Services;

public class DiscordClientService : IHostedService
{
    private readonly IDbContextFactory<MadsContext> _dbContextFactory;
    public readonly CommandsNextExtension CommandsNext;
    public readonly DiscordClient DiscordClient;
    public readonly LoggingService Logging;
    public readonly SlashCommandsExtension SlashCommands;
    public DateTime StartTime;
    
    private static ILogger _logger = Log.ForContext<DiscordClientService>();

    public DiscordClientService
    (
        MadsConfig pConfig,
        IDbContextFactory<MadsContext> dbDbContextFactory,
        LoggingService loggingService
    )
    {
        _logger.Warning("DiscordClientService");

        StartTime = DateTime.Now;
        MadsConfig config = pConfig;
        _dbContextFactory = dbDbContextFactory;
        Logging = loggingService;

        DiscordConfiguration discordConfig = new()
        {
            Token = config.Token,
            TokenType = TokenType.Bot,
            AutoReconnect = true,
            MinimumLogLevel = config.LogLevel,
            LoggerFactory = new LoggerFactory().AddSerilog(),
            Intents = DiscordIntents.All ^ DiscordIntents.GuildPresences
        };

        DiscordClient = new DiscordClient(discordConfig);
        EventListener.GuildDownload(DiscordClient);
        DiscordClient.GuildCreated += EventListener.OnGuildCreated;
        DiscordClient.GuildDeleted += EventListener.OnGuildDeleted;
        DiscordClient.GuildAvailable += EventListener.OnGuildAvailable;
        DiscordClient.ComponentInteractionCreated += EventListener.ActionButtons;
        DiscordClient.ComponentInteractionCreated += EventListener.OnRoleSelection;

        Assembly asm = Assembly.GetExecutingAssembly();

        //CNext
        CommandsNextConfiguration cnextConfig = new()
        {
            CaseSensitive = false,
            DmHelp = false,
            EnableDms = true,
            EnableMentionPrefix = true
        };
        CommandsNext = DiscordClient.UseCommandsNext(cnextConfig);
        CommandsNext.RegisterCommands(asm);
        CommandsNext.CommandErrored += EventListener.OnCNextErrored;

        //Slashcommands
        SlashCommandsConfiguration slashConfig = new()
        {
            Services = ModularDiscordBot.Services
        };
        SlashCommands = DiscordClient.UseSlashCommands(slashConfig);
#if RELEASE
        SlashCommands.RegisterCommands(asm);
#else
        _logger.Warning("SlashCommands are registered in debug mode");
        SlashCommands.RegisterCommands(asm, 938120155974750288);
#endif
        SlashCommands.SlashCommandErrored += EventListener.OnSlashCommandErrored;
        SlashCommands.AutocompleteErrored += EventListener.OnAutocompleteError;

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

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Warning("DiscordClientService started");
        
        //Update database to latest migration
        await using MadsContext context = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        IEnumerable<string> pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
        if (pendingMigrations.Any())
        {
            Stopwatch sw = new();
            sw.Start();
            await context.Database.MigrateAsync(cancellationToken);
            sw.Stop();
            _logger.Warning("Applied pending migrations in {Time} ms", sw.ElapsedMilliseconds);
        }
        
        DiscordActivity act = new("Messing with code", ActivityType.Custom);
        
        //connect client
        await DiscordClient.ConnectAsync(act, UserStatus.Online);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return DiscordClient.DisconnectAsync();
    }

    private Task OnGuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
    {
        Logging.Setup(this);
        return Task.CompletedTask;
    }
}