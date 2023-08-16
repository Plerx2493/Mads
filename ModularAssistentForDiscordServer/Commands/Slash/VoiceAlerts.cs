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
using MADS.Entities;
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
        [Option("repeat", "repeat the alert")] 
        bool repeat = false
    )
    {
        await VoiceAlertService.AddVoiceAlertAsync(ctx.User.Id, channel.Id, ctx.Guild.Id);

        var response = new DiscordInteractionResponseBuilder()
            .WithContent($"Added <#{channel.Id}> to your voicealerts")
            .AsEphemeral();

        await ctx.CreateResponseAsync(response);
    }

    [SlashCommand("remove", "remove a voicealert")]
    public async Task RemoveAlert(InteractionContext ctx,
        [Option("channel", "channel which will not be monitored anymore")] DiscordChannel channel)
    {
        await VoiceAlertService.RemoveVoiceAlert(ctx.User.Id, channel.Id, ctx.Guild.Id);
    }

    [SlashCommand("list", "list all voicealerts")]
    public async Task ListAlerts(InteractionContext ctx)
    {
        var alerts = await VoiceAlertService.GetVoiceAlerts(ctx.User.Id);
        var builder = new StringBuilder();
        foreach (var alert in alerts)
        {
            builder.AppendLine($"<#{alert.ChannelId}>");
        }

        await ctx.RespondAsync(builder.ToString());
    }
}