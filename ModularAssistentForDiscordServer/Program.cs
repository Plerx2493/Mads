using Microsoft.Extensions.Logging;

namespace MADS
{
    internal static class MainProgram
    {
        public static void Main()
        {
            while (true)
            {
                ModularDiscordBot modularDiscordBot = new();

                try
                {
                    modularDiscordBot.RunAsync().GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    modularDiscordBot.Logging.LogToOwner(string.Concat("**", e.GetType().ToString(), "**: ", e.Message), "core", LogLevel.Critical);
                }
            }
        }
    }
}