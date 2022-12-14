using System.Security.Policy;
using DSharpPlus;
using DSharpPlus.Entities;
using MADS.Entities;
using MADS.Extensions;
using MADS.JsonModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;

namespace MADS;

internal static class MainProgram
{
    public static void Main()
    {
        //Create cancellationToken and hook the cancelKey
        var cancellationSource = new CancellationTokenSource();
        Console.CancelKeyPress += (_, args) =>
        {
            args.Cancel = true;
            cancellationSource.Cancel();
        };
        
        
        
        //TODO Fix linux bug
        //Validate the config.json and create a new one when its not present/valid
        /*
        if (!ValidateConfig())
        {
            CreateConfig();
            return;
        }
        */
        
        //retrieves the config.json
        var config = DataProvider.GetConfig();

        //loop while the bot shouldn't be canceled
        while (!cancellationSource.IsCancellationRequested)
        {
            //Create a new instance of the bot
            ModularDiscordBot modularDiscordBot = new();
            
            //execute the bot and catch uncaught exceptions
            try
            {
                modularDiscordBot.RunAsync(config, cancellationSource.Token).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                if(e is TaskCanceledException) return;
                
                LogToWebhookAsync(e);
            }
            
            Task.Delay(10_000, cancellationSource.Token).GetAwaiter().GetResult();
        }
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
                             .WithColor(new DiscordColor(0,255,194))
                             .WithTimestamp(DateTime.UtcNow)
                             .WithTitle($"Ooopsie...  {e.GetType()}")
                             .WithDescription(e.Message);
                
        var webhookBuilder = new DiscordWebhookBuilder()
                             .WithUsername("Mads-Debug")
                             .AddEmbed(exceptionEmbed);
                
        await webhookClient.BroadcastMessageAsync(webhookBuilder);
    }

    private static bool ValidateConfig()
    {
        var configPath = DataProvider.GetPath("config.json");

        if (!File.Exists(configPath)) { return false; }

        var lConfig = DataProvider.GetConfig();

        if (lConfig.Token is null or "" or "<Your Token here>") { return false; }
        if (lConfig.Prefix is null or "") { lConfig.Prefix = "!"; }
        //if (lConfig.ConnectionString is null or "") return false;
        if (lConfig.DiscordWebhook is null or "") return false;

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
        };
        JsonProvider.ParseJson(configPath, newConfig);
    
        Console.WriteLine("Please insert your token in the config file and restart");
        Console.WriteLine("Filepath: " + configPath);
        Console.WriteLine("Press key to continue");
        Console.Read();
    }
    
    /*
    public class MadsContextFactory : IDesignTimeDbContextFactory<MadsContext>
    {
        public MadsContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MadsContext>();
            var connectionString = DataProvider.GetConfig().ConnectionString;
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new MadsContext(optionsBuilder.Options);
        }
    }
    */
}