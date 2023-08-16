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
    public VoiceAlertService VoiceAlertService => ModularDiscordBot.Services.GetRequiredService<VoiceAlertService>();

    [SlashCommand("add", "add a voicealert")]
    public async Task AddAlert
    (
        InteractionContext ctx,
        [Option("channel", "channel which will be monitored")]
        DiscordChannel channel,
        [Option("repeat", "repeat the alert")] bool repeat = false
    )
    {
        if (channel.Type is not (ChannelType.Voice or ChannelType.Stage))
        {
            var responseIsNotVoice = new DiscordInteractionResponseBuilder()
                .WithContent($"<#{channel.Id}> is not a voice channel")
                .AsEphemeral();

            await ctx.CreateResponseAsync(responseIsNotVoice);
            return;
        }

        var currentAlerts = await VoiceAlertService.GetVoiceAlerts(ctx.User.Id);
        if (currentAlerts.Any(x => x.ChannelId == channel.Id))
        {
            var responseIsActive = new DiscordInteractionResponseBuilder()
                .WithContent($"<#{channel.Id}> is already in your VoiceAlerts")
                .AsEphemeral();

            await ctx.CreateResponseAsync(responseIsActive);
            return;
        }

        await VoiceAlertService.AddVoiceAlertAsync(ctx.User.Id, channel.Id, ctx.Guild.Id, repeat);

        var response = new DiscordInteractionResponseBuilder()
            .WithContent($"Added <#{channel.Id}> to your VoiceAlerts")
            .AsEphemeral();

        await ctx.CreateResponseAsync(response);
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
            var responseIsNotVoice = new DiscordInteractionResponseBuilder()
                .WithContent($"\"{channel}\" is not a valid id")
                .AsEphemeral();

            await ctx.CreateResponseAsync(responseIsNotVoice);
            return;
        }

        var currentAlerts = await VoiceAlertService.GetVoiceAlerts(ctx.User.Id);
        if (!currentAlerts.Any(x => x.ChannelId == id))
        {
            var responseIsActive = new DiscordInteractionResponseBuilder()
                .WithContent($"<#{id}> is not in your VoiceAlerts")
                .AsEphemeral();

            await ctx.CreateResponseAsync(responseIsActive);
            return;
        }

        await VoiceAlertService.RemoveVoiceAlert(ctx.User.Id, id, ctx.Guild.Id);

        var response = new DiscordInteractionResponseBuilder()
            .WithContent($"Removed <#{channel}> from your VoiceAlerts")
            .AsEphemeral();

        await ctx.CreateResponseAsync(response);
    }

    [SlashCommand("list", "list all voicealerts")]
    public async Task ListAlerts(InteractionContext ctx)
    {
        var alerts = await VoiceAlertService.GetVoiceAlerts(ctx.User.Id);
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