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
using MADS.JsonModel;
using MADS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MADS;

public class ModularDiscordBot
{
    private CancellationToken _cancellationToken;
    private ConfigJson _config;

    public ModularDiscordBot()
    {
        _config = DataProvider.GetConfig();
    }

    public async Task<bool> RunAsync(CancellationToken token)
    {
        _cancellationToken = token;

        await Host.CreateDefaultBuilder()
            .UseSerilog()
            .UseConsoleLifetime()
            .ConfigureServices((hostContext, services) =>
                {
                    services
                        .AddLogging(logging => logging.ClearProviders().AddSerilog())
                        .AddSingleton(DataProvider.GetConfig())
                        .AddSingleton<DiscordClientService>()
                        .AddSingleton(s => s.GetRequiredService<DiscordClientService>().DiscordClient)
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
                        .AddSingleton(s =>
                            new TokenListener("51151", s.GetRequiredService<DiscordClient>(), "/api/v1/mads/token/"))
                        .AddHostedService(s => s.GetRequiredService<TokenListener>())
                        .AddSingleton<ReminderService>()
                        .AddHostedService(s => s.GetRequiredService<ReminderService>());
                }
            )
            .RunConsoleAsync();
        return true;
    }
}