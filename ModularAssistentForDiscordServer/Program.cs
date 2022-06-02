using MADS.Extensions;
using MADS.JsonModel;
using System;

namespace MADS
{
    internal class MainProgram
    {
        public static void Main()
        {
            ModularDiscordBot modularDiscordBot = new();
            modularDiscordBot.RunAsync().GetAwaiter().GetResult();
        }
    }
}