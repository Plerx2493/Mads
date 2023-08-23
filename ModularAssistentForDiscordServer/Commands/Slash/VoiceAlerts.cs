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

using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MADS.Commands.AutoCompletion;
using MADS.Extensions;
using MADS.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MADS.Commands.Slash;

[SlashCommandGroup("voicealerts", "mangage voicealerts")]
public class VoiceAlerts : MadsBaseApplicationCommand
{
    private VoiceAlertService _voiceAlertService;

    public VoiceAlerts(VoiceAlertService voiceAlertService)
    {
        _voiceAlertService = voiceAlertService;
    }
    
    [SlashCommand("add", "add a voicealert")]
    public async Task AddAlert
    (
        InteractionContext ctx,
        [Option("channel", "channel which will be monitored")]
        DiscordChannel channel,
        [Option("minTimeBetween", "time which has to pass between alerts")] 
        TimeSpan? minTimeBetween,
        [Option("repeat", "repeat the alert")] 
        bool repeat = false
    )
    {
        if (channel.Type is not (ChannelType.Voice or ChannelType.Stage))
        {
            await CreateResponse_Error($"<#{channel.Id}> is not a voice channel", true);
            return;
        }
        
        if (minTimeBetween is null)
        {
            await CreateResponse_Error("Invalid timespan (5s, 3m, 7h, 2d) - Use 0s if you want to get a alert everytime (Warning: This could lead to Spam)", true);
            return;
        }

        var currentAlerts = await _voiceAlertService.GetVoiceAlerts(ctx.User.Id);
        if (currentAlerts.Any(x => x.ChannelId == channel.Id))
        {
            await CreateResponse_Error($"<#{channel.Id}> is already in your VoiceAlerts", true);
            return;
        }

        await _voiceAlertService.AddVoiceAlertAsync(ctx.User.Id, channel.Id, ctx.Guild.Id, repeat, minTimeBetween.Value);

        await CreateResponse_Success($"Added <#{channel.Id}> to your VoiceAlerts", true);
    }

    [SlashCommand("delete", "delete a voicealerts")]
    public async Task RemoveAlert
    (
        InteractionContext ctx,
        [Option("channel", "channel which will not be monitored anymore", true),
         Autocomplete(typeof(VoiceAlertAutoCompletion))]
        string channel
    )
    {
        var isId = ulong.TryParse(channel, out var id);
        if (!isId)
        {
            await CreateResponse_Error($"**{channel}** is not a valid id", true);
            return;
        }

        var currentAlerts = await _voiceAlertService.GetVoiceAlerts(ctx.User.Id);
        if (!currentAlerts.Any(x => x.ChannelId == id))
        {
            await CreateResponse_Error($"<#{id}> is not in your VoiceAlerts", true);
            return;
        }

        await _voiceAlertService.RemoveVoiceAlert(ctx.User.Id, id, ctx.Guild.Id);

        await CreateResponse_Success($"Removed <#{channel}> from your VoiceAlerts", true);
    }

    [SlashCommand("list", "list all voicealerts")]
    public async Task ListAlerts(InteractionContext ctx)
    {
        var alerts = await _voiceAlertService.GetVoiceAlerts(ctx.User.Id);
        var builder = new StringBuilder();
        foreach (var alert in alerts)
        {
            builder.AppendLine($"<#{alert.ChannelId}> {(alert.IsRepeatable ? "repeated" : "")}");
        }

        if (builder.Length == 0)
        {
            builder.AppendLine("You have no VoiceAlerts");
        }

        await ctx.CreateResponseAsync(builder.ToString(), true);
    }
}