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
using DSharpPlus.Extensions;
using DSharpPlus.Net;
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
            .ConfigureServices((_, services) =>
                {
                    MadsConfig config = DataProvider.GetConfig();
                    services
                        .AddLogging(logging => logging.ClearProviders().AddSerilog())
                        .AddSingleton(config)
                        .AddDiscordClient(config.Token, DiscordIntents.All ^ DiscordIntents.GuildPresences)
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
                        .AddSingleton<MessageSnipeService>()
                        .AddHostedService(s => s.GetRequiredService<MessageSnipeService>())
                        .AddSingleton<QuotesService>()
                        .AddSingleton<StarboardService>()
                        .AddHostedService(s => s.GetRequiredService<StarboardService>())
                        .AddSingleton<ReminderService>()
                        .AddHostedService(s => s.GetRequiredService<ReminderService>())
                        .AddSingleton<VoiceAlertService>()
                        .AddHostedService(s => s.GetRequiredService<VoiceAlertService>())
                        .AddSingleton(new Translator(_config.DeeplApiKey ?? ""))
                        .AddSingleton<TranslateInformationService>()
                        .AddSingleton<LoggingService>()
                        .AddSingleton<UserManagerService>()
                        .AddHttpClient();
                    
                    Services = services.BuildServiceProvider();
                }
            )
            .RunConsoleAsync();
    }
}