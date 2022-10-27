using MADS.Entities;
using MADS.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging;

namespace MADS;

internal static class MainProgram
{
    public static void Main()
    {
        var cancellationSource = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, args) =>
        {
            args.Cancel = true;
            cancellationSource.Cancel();
        };
        
        while (!cancellationSource.IsCancellationRequested)
        {
            ModularDiscordBot modularDiscordBot = new();
            
            try
            {
                modularDiscordBot.RunAsync(cancellationSource.Token).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                //modularDiscordBot.Logging.LogToOwner(string.Concat("**", e.GetType().ToString(), "**: ", e.Message), "core", LogLevel.Critical);
            }
        }
    }

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
}