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

using System.Diagnostics;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using MADS.Commands.ContextMenu;
using MADS.Commands.Slash;
using MADS.Commands.Text.Base;
using MADS.CommandsChecks;
using MADS.Entities;
using MADS.EventListeners;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace MADS.Services;

public class DiscordCommandService : IHostedService
{
    public readonly IDbContextFactory<MadsContext> DbContextFactory;
    public readonly CommandsExtension Commands;
    public readonly DiscordClient DiscordClient;
    public DateTime StartTime;
    
    private static readonly ILogger _logger = Log.ForContext<DiscordCommandService>();
    
    public DiscordCommandService
    (
        DiscordClient discordClient,
        IDbContextFactory<MadsContext> dbContextFactory
    )
    {
        DiscordClient = discordClient;
        StartTime = DateTime.Now;
        DbContextFactory = dbContextFactory;
        
        List<Type> commandTypes =
        [
            typeof(StealEmojiMessage), typeof(TranslateMessage), typeof(UserInfoUser), typeof(About), typeof(BotStats),
            typeof(Jumppad), typeof(MessageSnipe), typeof(MoveEmoji), typeof(Ping), typeof(Purge), typeof(Quotes),
            typeof(Reminder), typeof(RoleSelection), typeof(StarboardConfig), typeof(Translation), typeof(VoiceAlerts),
            typeof(Eval), typeof(ExitGuild)
        ];
        
        
        CommandsConfiguration commandsConfiguration = new()
        {
#if !RELEASE
            DebugGuildId = 938120155974750288
#endif
        };
        Commands = DiscordClient.UseCommands(commandsConfiguration);
        Commands.AddCommands(commandTypes);
        Commands.AddCheck<EnsureDBEntitiesCheck>();
        Commands.CommandErrored += EventListener.OnCommandsErrored;
        
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
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.Warning("DiscordClientService started");
        
        //Update database to latest migration
        await using MadsContext context = await DbContextFactory.CreateDbContextAsync(cancellationToken);
        IEnumerable<string> pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
        if (pendingMigrations.Any())
        {
            Stopwatch sw = new();
            sw.Start();
            await context.Database.MigrateAsync(cancellationToken);
            sw.Stop();
            _logger.Warning("Applied pending migrations in {Time} ms", sw.ElapsedMilliseconds);
        }
        
        DiscordActivity act = new("Messing with code", DiscordActivityType.Custom);
        
        //connect client
        await DiscordClient.ConnectAsync(act, DiscordUserStatus.Online);
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return DiscordClient.DisconnectAsync();
    }
}