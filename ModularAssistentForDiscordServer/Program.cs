using MADS.Commands;
using MADS.Entities;
using MADS.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MADS;

internal static class MainProgram
{
    public static void Main()
    {
        ModularDiscordBot modularDiscordBot = new();
        modularDiscordBot.RunAsync().GetAwaiter().GetResult();
        try
        {
        }
        catch (Exception e)
        {
            modularDiscordBot.Logging.LogToOwner(string.Concat("**", e.GetType().ToString(), "**: ", e.Message),
                "core", LogLevel.Critical);
        }
        
        Main();
    }
    
    public class MadsContextFactory : IDesignTimeDbContextFactory<MadsContext>
    {
        public MadsContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MadsContext>();
            var connectionString = DataProvider.GetConfig().ConnectionString;   //;
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new MadsContext(optionsBuilder.Options);
        }
    }
}