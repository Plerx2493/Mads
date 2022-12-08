using DSharpPlus;
using DSharpPlus.EventArgs;
using MADS.Extensions;

namespace MADS.EventListeners;

internal static partial class EventListener
{
    internal static async Task DmHandler(DiscordClient client, MessageCreateEventArgs e)
    {
        if (!e.Channel.IsPrivate) return;
        
        var webhook = DataProvider.GetConfig().DiscordWebhook;
        
        //TODO Add DmHandler
    }
}