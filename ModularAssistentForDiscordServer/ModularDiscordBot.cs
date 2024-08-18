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

using DeepL;
using DSharpPlus;
using DSharpPlus.Clients;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Processors.TextCommands.Parsing;
using DSharpPlus.Extensions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Net;
using MADS.Commands.ContextMenu;
using MADS.Commands.Slash;
using MADS.Commands.Text.Base;
using MADS.CommandsChecks;
using MADS.Entities;
using MADS.EventListeners;
using MADS.Extensions;
using MADS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Serilog;

namespace MADS;

public class ModularDiscordBot
{
    public static IServiceProvider Services;
    private readonly MadsConfig _config = DataProvider.GetConfig();
    
    public async Task RunAsync()
    {
        await Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices((ctx, services) =>
                {
                    MadsConfig config = DataProvider.GetConfig();
                    services
                        .AddLogging(logging =>
                        {
                            logging
                                .SetMinimumLevel(LogLevel.Trace)
                                .AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning)
                                .AddFilter("Quartz", LogLevel.Warning)
                                .AddConsole();
                        })
                        .AddSingleton(config)
                        .AddDiscordClient(config.Token, DiscordIntents.All ^ DiscordIntents.GuildPresences)
                        .AddCommandsExtension(extension =>
                        {
                            List<Type> commandTypes =
                            [
                                typeof(StealEmojiMessage), typeof(TranslateMessage), typeof(UserInfoUser), typeof(About), typeof(BotStats),
                                typeof(Jumppad), typeof(MessageSnipe), typeof(MoveEmoji), typeof(Ping), typeof(Purge), typeof(Quotes),
                                typeof(Reminder), typeof(RoleSelection), typeof(StarboardConfig), typeof(Translation), typeof(VoiceAlerts),
                                typeof(Eval), typeof(ExitGuild)
                            ];
                            
                            extension.AddProcessors(new TextCommandProcessor(new TextCommandConfiguration()
                            {
                                PrefixResolver = new DefaultPrefixResolver(true, config.Prefix).ResolvePrefixAsync
                            }));
                            extension.AddCommands(commandTypes);
                            extension.AddCheck<EnsureDBEntitiesCheck>();
                            extension.CommandErrored += EventListener.OnCommandsErrored;
                        })
                        .AddInteractivityExtension(new InteractivityConfiguration
                        {
                            PollBehaviour = PollBehaviour.KeepEmojis,
                            Timeout = TimeSpan.FromMinutes(10),
                            ButtonBehavior = ButtonPaginationBehavior.DeleteButtons,
                            PaginationBehaviour = PaginationBehaviour.Ignore,
                            ResponseBehavior = InteractionResponseBehavior.Ignore,
                            ResponseMessage = "invalid interaction",
                            PaginationDeletion = PaginationDeletion.DeleteEmojis
                        })
                        .AddDiscordRestClient(config)
                        .ConfigureEventHandlers(x =>
                        {
                            x.HandleGuildDownloadCompleted(EventListener.UpdateDb);
                            x.HandleGuildCreated(EventListener.OnGuildCreated);
                            x.HandleGuildDeleted(EventListener.OnGuildDeleted);
                            x.HandleGuildAvailable(EventListener.OnGuildAvailable);
                            x.HandleComponentInteractionCreated(EventListener.ActionButtons);
                            x.HandleComponentInteractionCreated(EventListener.OnRoleSelection);
                            x.HandleZombied(EventListener.OnZombied);
                            x.HandleMessageCreated(EventListener.DmHandler);

                            x.AddEventHandlers<LoggingService>(ServiceLifetime.Singleton);
                            x.AddEventHandlers<MessageSnipeService>(ServiceLifetime.Singleton);
                            x.AddEventHandlers<StarboardService>(ServiceLifetime.Singleton);
                            x.AddEventHandlers<VoiceAlertService>(ServiceLifetime.Singleton);
                            x.AddEventHandlers<AntiPhishingService>();
                        })
                        .Configure<RestClientOptions>(x =>
                        {
                            x.Timeout = TimeSpan.FromSeconds(30);
                        })
                        .AddSingleton<DiscordCommandService>()
                        .AddHostedService(s => s.GetRequiredService<DiscordCommandService>())
                        .AddDbContextFactory<MadsContext>(
                            options =>
                            {
                                options.UseMySql(_config.ConnectionString,
                                    ServerVersion.AutoDetect(_config.ConnectionString));
                                options.EnableDetailedErrors();
                            }
                        )
                        .AddMemoryCache()
                        .AddQuartz(x =>
                        {
                            x.UsePersistentStore(options =>
                            {
                                options.UseMySqlConnector(opt =>
                                {
                                    opt.ConnectionString = _config.ConnectionStringQuartz;
                                    opt.TablePrefix = "QRTZ_";
                                });
                                options.UseNewtonsoftJsonSerializer();
                            });
                            x.InterruptJobsOnShutdownWithWait = true;
                            x.UseSimpleTypeLoader();
                        })
                        .AddQuartzHostedService(options =>
                        {
                            options.WaitForJobsToComplete = true;
                            options.StartDelay = TimeSpan.FromSeconds(10);
                            options.AwaitApplicationStarted = true;
                        })
                        .AddSingleton<QuotesService>()
                        .AddHostedService(s => s.GetRequiredService<StarboardService>())
                        .AddSingleton<ReminderService>()
                        .AddHostedService(s => s.GetRequiredService<ReminderService>())
                        .AddHostedService(s => s.GetRequiredService<VoiceAlertService>())
                        .AddSingleton(new Translator(_config.DeeplApiKey ?? ""))
                        .AddSingleton<TranslateInformationService>()
                        .AddSingleton<UserManagerService>()
                        .AddHttpClient();
                    
                    Services = services.BuildServiceProvider();
                }
            )
            .RunConsoleAsync();
    }
}