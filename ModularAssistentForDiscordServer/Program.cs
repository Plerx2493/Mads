using Microsoft.Extensions.Logging;

namespace MADS
{
    internal class MainProgram
    {
        public static void Main()
        {
            ModularDiscordBot modularDiscordBot = new();
            
            try
            {
                modularDiscordBot.RunAsync().GetAwaiter().GetResult();
            }
            catch(Exception e)
            {
                modularDiscordBot.Logging.LogToOwner($"Bot crashed: {e.Message}", "core", LogLevel.Critical);
            }

            Main();
        }
    }
}