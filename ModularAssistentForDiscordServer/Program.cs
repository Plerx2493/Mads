using MADS.Commands;
using MADS.Entities;
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
            var connectionString = "Server=192.168.178.61,Port=3306;Database=MadsDB;User=USR;Password=PWD;";
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new MadsContext(optionsBuilder.Options);
        }
    }
}