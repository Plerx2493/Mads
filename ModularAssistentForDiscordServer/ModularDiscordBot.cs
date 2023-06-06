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

using DSharpPlus;
using MADS.Extensions;
using MADS.JsonModel;
using MADS.Services;
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
    private readonly ConfigJson _config;

    public ModularDiscordBot()
    {
        _config = DataProvider.GetConfig();
    }

    public async Task<bool> RunAsync(CancellationToken token)
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
                        .AddDbFactoryDebugOrRelease(_config)
                        .AddDiscordRestClient(_config)
                        .AddMemoryCache(options =>
                        {
                            options.ExpirationScanFrequency = TimeSpan.FromMinutes(10);
                            options.SizeLimit = 1024L;
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
                                options.UseJsonSerializer();
                            });
                            x.InterruptJobsOnShutdownWithWait = true;
                            x.UseMicrosoftDependencyInjectionJobFactory();
                            x.UseSimpleTypeLoader();
                            x.SchedulerName = "reminder-scheduler";
                        })
                        .AddQuartzHostedService(options =>
                        {
                            // when shutting down we want jobs to complete gracefully
                            options.WaitForJobsToComplete = true;
                        })
                        .AddSingleton<VolatileMemoryService>()
                        .AddSingleton<QuotesService>()
                        .AddSingleton<StarboardService>()
                        .AddHostedService(s => s.GetRequiredService<StarboardService>())
                        .AddSingleton(s =>
                            new TokenListener("51151", s.GetRequiredService<DiscordClient>(), "/api/v1/mads/token/"))
                        .AddHostedService(s => s.GetRequiredService<TokenListener>())
                        .AddSingleton<ReminderService>()
                        .AddHostedService(s => s.GetRequiredService<ReminderService>());

                    Services = services.BuildServiceProvider();
                }
            )
            .RunConsoleAsync();
        return true;
    }
}