using DSharpPlus;
using DSharpPlus.EventArgs;
using MADS.Services;

namespace MADS.EventListeners;

internal static partial class EventListener
{
    public static Task VoiceTrollListener(DiscordClient client, VolatileMemoryService memory)
    {
        client.VoiceStateUpdated += Task(_, e) =>
        {
            if (e.After is null || e.Before is not null) return Task.CompletedTask;
            if (!memory.VoiceTroll.Active(e.User)) return Task.CompletedTask;
            Task.Run(() => TrollUser(e));
            return Task.CompletedTask;
        };
        return Task.CompletedTask;
    }

    private static async Task TrollUser(VoiceStateUpdateEventArgs eventArgs)
    {
        var rnd = new Random();
        var delay = rnd.Next(1_000, 10_001);
        await Task.Delay(delay);

        var usr = await eventArgs.Guild.GetMemberAsync(eventArgs.User.Id);
        await usr.ModifyAsync(x => x.VoiceChannel = null);
    }
}