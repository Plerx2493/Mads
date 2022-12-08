using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Emzi0767.Utilities;
using MADS.Services;

namespace MADS.EventListeners;

internal static partial class EventListener 
{
    public static async Task VoiceTrollListener(DiscordClient client, VolatileMemoryService memory)
    {
        
        client.VoiceStateUpdated += async Task(s, e) =>
        {
            if (e.After is null || e.Before is not null) return;
            if (!memory.IsVoiceTrollUser(e.User)) return;
            Task.Run(() => TrollUser(s, e));
        };
    }

    private static async Task TrollUser(DiscordClient sender, VoiceStateUpdateEventArgs eventArgs)
    {
        Random rnd = new Random();
        int delay  = rnd.Next(1_000, 10_001);
        await Task.Delay(delay);
        
        var usr = await eventArgs.Guild.GetMemberAsync(eventArgs.User.Id);
        await usr.ModifyAsync(x => x.VoiceChannel = null);
    }
}