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

using System.Diagnostics.CodeAnalysis;
using DSharpPlus;
using DSharpPlus.Entities;
using MADS.Entities;
using MADS.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace MADS;

internal static class MainProgram
{
    public static DiscordWebhookClient WebhookClient = null!;
    
    public static async Task Main()
    {
        await Task.Delay(20_000); //Delay to give the databases time to start
        
        Log.Logger = new LoggerConfiguration()
           .WriteTo.Console()
           .MinimumLevel.Verbose() 
           .MinimumLevel.Override("Quartz", LogEventLevel.Warning)
           .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
           .CreateLogger();
        
        //retrieves the config.json
        MadsConfig config = DataProvider.GetConfig();
        
        //Create a discordWebhookClient and add the debug webhook from the config.json
        WebhookClient = new DiscordWebhookClient();
        Uri webhookUrl = new(config.DiscordWebhook);
        await WebhookClient.AddWebhookAsync(webhookUrl);
        
        //Create a new instance of the bot
        ModularDiscordBot modularDiscordBot = new();
        //execute the bot and catch uncaught exceptions
        try
        {
            await modularDiscordBot.RunAsync();
        }
        catch (Exception e)
        {
            if (e is TaskCanceledException)
            {
                return;
            }
            
            Log.Error(e, "An uncaught exception occurred");
            Task _ = LogToWebhookAsync(e);
            Environment.Exit(69);
        }
    }
    
    public static async Task LogToWebhookAsync(Exception e)
    {
        DiscordEmbedBuilder exceptionEmbed = new DiscordEmbedBuilder()
            .WithAuthor("Mads-Debug")
            .WithColor(DiscordColor.Red)
            .WithTimestamp(DateTime.UtcNow)
            .WithTitle($"Ooopsie...  {e.GetType()}")
            .WithDescription(e.Message);
        
        DiscordWebhookBuilder webhookBuilder = new DiscordWebhookBuilder()
            .WithUsername("Mads-Debug")
            .AddEmbed(exceptionEmbed);
        
        await WebhookClient.BroadcastMessageAsync(webhookBuilder);
    }
    
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "EFCore CLI tools rely on reflection.")]
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        IHostBuilder builder = Host.CreateDefaultBuilder(args);
        //MadsConfig config = DataProvider.GetConfig();
        string connectionString = "Server=192.168.178.61;Database=MadsDBDev;Uid=root;Pwd=owsip#63;";
        
        builder.ConfigureServices((_, services) => services.AddDbContextFactory<MadsContext>(
            options => options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(
                    connectionString))
        ));
        return builder;
    }
}