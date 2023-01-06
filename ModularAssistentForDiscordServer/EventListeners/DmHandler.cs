using DSharpPlus;
using DSharpPlus.EventArgs;
using MADS.Extensions;

namespace MADS.EventListeners;

internal static partial class EventListener
{
    internal static Task DmHandler(DiscordClient client, MessageCreateEventArgs e)
    {
        if (!e.Channel.IsPrivate) return Task.CompletedTask;

        var webhook = DataProvider.GetConfig().DiscordWebhook;

        //TODO Add DmHandler
        return Task.CompletedTask;
    }
}