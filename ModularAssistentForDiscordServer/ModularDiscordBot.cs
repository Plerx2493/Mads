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

using DeepL;
using MADS.Entities;
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
    public static DateTimeOffset StartTime = DateTimeOffset.Now;
    public static ILogger<ModularDiscordBot> Logger;
    private readonly MadsConfig _config = DataProvider.GetConfig();

    public async Task RunAsync(CancellationToken token)
    {
        await Host.CreateDefaultBuilder()
            .UseSerilog()
            .UseConsoleLifetime()
            .ConfigureServices((_, services) =>
                {
                    services
                        .AddLogging(logging => logging.ClearProviders().AddSerilog())
                        .AddSingleton(DataProvider.GetConfig())
                        .AddSingleton<DiscordClientService>()
                        .AddHostedService(s => s.GetRequiredService<DiscordClientService>())
                        .AddSingleton(s => s.GetRequiredService<DiscordClientService>().DiscordClient)
                        .AddDbContextFactory<MadsContext>(
                            options =>
                            {
                                options.UseMySql(_config.ConnectionString, ServerVersion.AutoDetect(_config.ConnectionString));
                                options.EnableDetailedErrors();
                            }
                        )
                        .AddDiscordRestClient(_config)
                        .AddMemoryCache(options =>
                        {
                            options.ExpirationScanFrequency = TimeSpan.FromMinutes(1);
                            options.SizeLimit = 4096L;
                        })
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
                            x.SchedulerName = "reminder-scheduler";
                        })
                        .AddQuartzHostedService(options =>
                        {
                            options.WaitForJobsToComplete = true;
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
                        .AddHttpClient();

                    Services = services.BuildServiceProvider();
                    Logger = Services.GetRequiredService<ILogger<ModularDiscordBot>>();
                }
            )
            .RunConsoleAsync(cancellationToken: token);
    }
}