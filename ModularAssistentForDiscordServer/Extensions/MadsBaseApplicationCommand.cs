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

using System.Diagnostics;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MADS.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MADS.Extensions;

[SlashModuleLifespan(SlashModuleLifespan.Transient)]
public class MadsBaseApplicationCommand : ApplicationCommandModule
{
    private InteractionContext? _ctx;
    private readonly Stopwatch _executionTimer = new();
    protected DiscordClientService CommandService => ModularDiscordBot.Services.GetRequiredService<DiscordClientService>();

    public override Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
    {
        _ctx = ctx;
        _executionTimer.Restart();

        return Task.FromResult(true);
    }

    public override Task AfterSlashExecutionAsync(InteractionContext ctx)
    {
        _executionTimer.Stop();

        _ = CommandService.Logging.LogCommandExecutionAsync(ctx, _executionTimer.Elapsed);


        return Task.CompletedTask;
    }

    public override Task<bool> BeforeContextMenuExecutionAsync(ContextMenuContext ctx)
    {
        _executionTimer.Restart();

        return Task.FromResult(true);
    }

    public override Task AfterContextMenuExecutionAsync(ContextMenuContext ctx)
    {
        _executionTimer.Stop();

        _ = CommandService.Logging.LogCommandExecutionAsync(ctx, _executionTimer.Elapsed);

        return Task.FromResult(true);
    }

    public async Task IntendedWait(int milliseconds)
    {
        _executionTimer.Stop();

        await Task.Delay(milliseconds);

        _executionTimer.Start();
    }
    
    protected async Task CreateResponse_Error(string message, bool isEphemeral = false)
    {
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithTitle("Error")
            .WithDescription(message)
            .WithColor(DiscordColor.Red);
        
        DiscordInteractionResponseBuilder response = new DiscordInteractionResponseBuilder()
            .AddEmbed(embed)
            .AsEphemeral(isEphemeral);

        await _ctx!.CreateResponseAsync(response);
    }
    
    protected async Task CreateResponse_Success(string message, bool isEphemeral = false)
    {
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithTitle("Success")
            .WithDescription(message)
            .WithColor(DiscordColor.Green);
        
        DiscordInteractionResponseBuilder response = new DiscordInteractionResponseBuilder()
            .AddEmbed(embed)
            .AsEphemeral(isEphemeral);

        await _ctx!.CreateResponseAsync(response);
    }

    protected async Task EditResponse_Error(string message)
    {
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithTitle("Error")
            .WithDescription(message)
            .WithColor(DiscordColor.Red);
        
        DiscordWebhookBuilder response = new DiscordWebhookBuilder()
            .AddEmbed(embed);

        await _ctx!.EditResponseAsync(response);
    }

    protected async Task EditResponse_Success(string message)
    {
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithTitle("Success")
            .WithDescription(message)
            .WithColor(DiscordColor.Green);

        DiscordWebhookBuilder response = new DiscordWebhookBuilder()
            .AddEmbed(embed);

        await _ctx!.EditResponseAsync(response);
    }
}