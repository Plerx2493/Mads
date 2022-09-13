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
}