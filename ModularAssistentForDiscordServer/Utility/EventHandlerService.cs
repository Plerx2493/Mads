using MADS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MADS.Utility
{
    internal class EventHandlerService
    {
        ModularDiscordBot ModularDiscordBot;

        public EventHandlerService(ModularDiscordBot modularDiscordBot)
        {
            ModularDiscordBot = modularDiscordBot;
        }
    }
}
