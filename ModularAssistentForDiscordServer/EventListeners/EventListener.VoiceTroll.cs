// Copyright 2023 Plerx2493
//
// Licensed under the Apache License, Version 2.0 (the "License")
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using DSharpPlus;
using DSharpPlus.EventArgs;
using MADS.Services;

namespace MADS.EventListeners;

internal static partial class EventListener
{
    public static void VoiceTrollListener(DiscordClient client, VolatileMemoryService memory)
    {
        client.VoiceStateUpdated += async Task(_, e) =>
        {
            if (e.After is null || e.Before is not null) return;
            if (!memory.VoiceTroll.Active(e.User)) return;
            await TrollUser(e);
        };
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