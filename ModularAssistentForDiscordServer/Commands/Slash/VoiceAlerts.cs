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

using System.ComponentModel;
using System.Text;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using MADS.Commands.AutoCompletion;
using MADS.Entities;
using MADS.Extensions;
using MADS.Services;

namespace MADS.Commands.Slash;

[Command("voicealerts"), Description("mangage voicealerts")]
public sealed class VoiceAlerts
{
    private readonly VoiceAlertService _voiceAlertService;

    public VoiceAlerts(VoiceAlertService voiceAlertService)
    {
        _voiceAlertService = voiceAlertService;
    }
    
    [Command("add"), Description("add a voicealert")]
    public async Task AddAlert
    (
        CommandContext ctx,
        [Description("channel which will be monitored"), SlashChannelTypes(DiscordChannelType.Voice, DiscordChannelType.Stage)]
        DiscordChannel channel,
        [Description("time which has to pass between alerts")] 
        TimeSpan? minTimeBetween,
        [Description("If the alert should be repeated or one shot. (Defaults to false (one shot))")] 
        bool repeat = false
    )
    {
        if (channel.Type is not (DiscordChannelType.Voice or DiscordChannelType.Stage))
        {
            await ctx.CreateResponse_Error($"<#{channel.Id}> is not a voice channel", true);
            return;
        }
        
        if (minTimeBetween is null)
        {
            await ctx.CreateResponse_Error("Invalid timespan (5s, 3m, 7h, 2d) - Use 0s if you want to get a alert everytime (Warning: This could lead to Spam)", true);
            return;
        }

        IEnumerable<VoiceAlert> currentAlerts = await _voiceAlertService.GetVoiceAlerts(ctx.User.Id);
        if (currentAlerts.Any(x => x.ChannelId == channel.Id))
        {
            await ctx.CreateResponse_Error($"<#{channel.Id}> is already in your VoiceAlerts", true);
            return;
        }

        await _voiceAlertService.AddVoiceAlertAsync(ctx.User.Id, channel.Id, ctx.Guild.Id, repeat, minTimeBetween.Value);

        await ctx.CreateResponse_Success($"Added <#{channel.Id}> to your VoiceAlerts", true);
    }

    [Command("delete"), Description("delete a voicealerts")]
    public async Task RemoveAlert
    (
        CommandContext ctx,
        [Description("channel which will not be monitored anymore"),
         SlashAutoCompleteProvider(typeof(VoiceAlertAutoCompletion))]
        string channel
    )
    {
        bool isId = ulong.TryParse(channel, out ulong id);
        if (!isId)
        {
            await ctx.CreateResponse_Error($"**{channel}** is not a valid id", true);
            return;
        }

        IEnumerable<VoiceAlert> currentAlerts = await _voiceAlertService.GetVoiceAlerts(ctx.User.Id);
        if (!currentAlerts.Any(x => x.ChannelId == id))
        {
            await ctx.CreateResponse_Error($"<#{id}> is not in your VoiceAlerts", true);
            return;
        }

        await _voiceAlertService.RemoveVoiceAlert(ctx.User.Id, id, ctx.Guild.Id);

        await ctx.CreateResponse_Success($"Removed <#{channel}> from your VoiceAlerts", true);
    }

    [Command("list"), Description("list all voicealerts")]
    public async Task ListAlerts(CommandContext ctx)
    {
        IEnumerable<VoiceAlert> alerts = await _voiceAlertService.GetVoiceAlerts(ctx.User.Id);
        StringBuilder builder = new();
        foreach (VoiceAlert alert in alerts)
        {
            builder.AppendLine($"<#{alert.ChannelId}> {(alert.IsRepeatable ? "repeated" : "")}");
        }

        if (builder.Length == 0)
        {
            builder.AppendLine("You have no VoiceAlerts");
        }
        
        DiscordInteractionResponseBuilder responseBuilder = new();
        responseBuilder.WithContent(builder.ToString()).AsEphemeral();

        await ctx.RespondAsync(responseBuilder);
    }
}