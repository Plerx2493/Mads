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
using MADS.Services;
using MADS.JsonModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MADS;

internal static class MainProgram
{
    public static void Main()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
        
        //Create cancellationToken and hook the cancelKey
        var cancellationSource = new CancellationTokenSource();
        Console.CancelKeyPress += (_, args) =>
        {
            args.Cancel = true;
            cancellationSource.Cancel();
        };

        //Validate the config.json and create a new one when its not present/valid
        if (!ValidateConfig())
        {
            CreateConfig();
            return;
        }

        //retrieves the config.json
        var config = DataProvider.GetConfig();

        //Create a discordWebhookClient and add the debug webhook from the config.json
        var webhookClient = new DiscordWebhookClient();
        var webhookUrl = new Uri(config.DiscordWebhook);
        webhookClient.AddWebhookAsync(webhookUrl).GetAwaiter().GetResult();

        //loop while the bot shouldn't be canceled
        //while (!cancellationSource.IsCancellationRequested)
        //{
            //Create a new instance of the bot
            ModularDiscordBot modularDiscordBot = new();
            //execute the bot and catch uncaught exceptions
            try
            {
                modularDiscordBot.RunAsync(cancellationSource.Token).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                if (e is TaskCanceledException) return;

                var _ = LogToWebhookAsync(e);
            }

            try
            {
                Task.Delay(10_000, cancellationSource.Token).GetAwaiter().GetResult();
            }
            catch (TaskCanceledException) { }
        //}
    }

    private static bool ValidateConfig()
    {
        var configPath = DataProvider.GetPath("config.json");

        if (!File.Exists(configPath))
        {
            return false;
        }

        var lConfig = DataProvider.GetConfig();

        if (lConfig.Token is null or "" or "<Your Token here>")
        {
            return false;
        }

        if (lConfig.Prefix is null or "")
        {
            lConfig.Prefix = "!";
        }

        if (lConfig.DiscordWebhook is null or "") return false;
        lConfig.DmProxyChannelId ??= 0;

        DataProvider.SetConfig(lConfig);

        return true;
    }

    private static void CreateConfig()
    {
        var configPath = DataProvider.GetPath("config.json");

        var fileStream = File.Create(configPath);
        fileStream.Close();

        ConfigJson newConfig = new()
        {
            Token = "<Your Token here>",
            Prefix = "!",
            LogLevel = LogLevel.Debug,
            DmProxyChannelId = 0
        };
        JsonProvider.ParseJson(configPath, newConfig);

        Console.WriteLine("Please insert your token in the config file and restart");
        Console.WriteLine("Filepath: " + configPath);
        Console.WriteLine("Press key to continue");
        Console.Read();
    }

    public static async Task LogToWebhookAsync(Exception e)
    {
        //retrieves the config.json
        var config = DataProvider.GetConfig();

        //Create a discordWebhookClient and add the debug webhook from the config.json
        var webhookClient = new DiscordWebhookClient();
        var webhookUrl = new Uri(config.DiscordWebhook);
        webhookClient.AddWebhookAsync(webhookUrl).GetAwaiter().GetResult();


        var exceptionEmbed = new DiscordEmbedBuilder()
            .WithAuthor("Mads-Debug")
            .WithColor(DiscordColor.Red)
            .WithTimestamp(DateTime.UtcNow)
            .WithTitle($"Ooopsie...  {e.GetType()}")
            .WithDescription(e.Message);

        var webhookBuilder = new DiscordWebhookBuilder()
            .WithUsername("Mads-Debug")
            .AddEmbed(exceptionEmbed);

        await webhookClient.BroadcastMessageAsync(webhookBuilder);
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "EFCore CLI tools rely on reflection.")]
    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);
        var config = DataProvider.GetConfig();

        builder.ConfigureServices((_, services) => services.AddDbContextFactory<MadsContext>(
            options => options.UseMySql(
                config.ConnectionString,
                ServerVersion.AutoDetect(
                    config.ConnectionString))
        ));
        return builder;
    }
}