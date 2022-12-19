using DSharpPlus;
using DSharpPlus.EventArgs;

namespace MADS.EventListeners;

internal static partial class EventListener
{
    internal static async Task OnZombied(DiscordClient sender, ZombiedEventArgs e)
    {
        await sender.ReconnectAsync(true);
    }
}