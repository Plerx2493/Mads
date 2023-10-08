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
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MADS.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace MADS.Commands.Text.Base;

public class ExitGuild : MadsBaseCommand
{
    [Command("exit"), Description("Exit the bot"), RequirePermissions(Permissions.ManageGuild), RequireGuild]
    public async Task ExitGuildCommand(CommandContext ctx)
    {
        await ctx.RespondAsync("Leaving server...");
        await ctx.Guild.LeaveAsync();
    }

    [Command("leave"), Description("Leave given server"), RequireGuild, Hidden, RequireOwner]
    public async Task LeaveGuildOwner(CommandContext ctx)
    {
        await ctx.Message.DeleteAsync();
        await ctx.Guild.LeaveAsync();
    }
    
    [Command("test"), Description("Leave given server"), RequireGuild, Hidden, RequireOwner]
    public async Task Test(CommandContext ctx, DiscordChannel chnl, int limit)
    {
        var client = ModularDiscordBot.Services.GetRequiredService<DiscordRestClient>();
        var channel = await client.GetChannelAsync(chnl.Id);
        var messages = channel.GetMessagesAfterAsync(ctx.Message.Id + 1221);

        messages.ToBlockingEnumerable().Count();
        
        
        int i = 0;
        await foreach (var message in messages)
        {
            i++;
        }
        
        await ctx.RespondAsync($"Found {i} messages");
        
    }
}