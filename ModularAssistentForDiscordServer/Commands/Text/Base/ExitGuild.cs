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
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;
using MADS.CommandsChecks;

namespace MADS.Commands.Text.Base;

public class ExitGuild
{
    [Command("leave"), Description("Leave given server"), RequireGuild, RequireApplicationOwner]
    public async Task LeaveGuildOwner(TextCommandContext ctx)
    {
        await ctx.Message.DeleteAsync();
        await ctx.Guild!.LeaveAsync();
    }
}