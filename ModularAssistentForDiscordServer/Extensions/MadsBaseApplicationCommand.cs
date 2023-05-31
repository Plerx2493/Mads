﻿// Copyright 2023 Plerx2493
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
using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace MADS.Services;

[SlashModuleLifespan(SlashModuleLifespan.Transient)]
public class MadsBaseApplicationCommand : ApplicationCommandModule
{
    private readonly Stopwatch         _executionTimer = new();
    public           DiscordClientService CommandService { get; set; }

    public override Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
    {
        _executionTimer.Restart();

        return Task.FromResult(true);
    }

    public override Task AfterSlashExecutionAsync(InteractionContext ctx)
    {
        _executionTimer.Stop();

        var _ = CommandService.Logging.LogCommandExecutionAsync(ctx, _executionTimer.Elapsed);


        return Task.FromResult(true);
    }

    public override Task<bool> BeforeContextMenuExecutionAsync(ContextMenuContext ctx)
    {
        _executionTimer.Restart();

        return Task.FromResult(true);
    }

    public override Task AfterContextMenuExecutionAsync(ContextMenuContext ctx)
    {
        _executionTimer.Stop();

        var _ = CommandService.Logging.LogCommandExecutionAsync(ctx, _executionTimer.Elapsed);

        return Task.FromResult(true);
    }

    public async Task IntendedWait(int milliseconds)
    {
        _executionTimer.Stop();

        await Task.Delay(milliseconds);
        
        _executionTimer.Start();
    }
}