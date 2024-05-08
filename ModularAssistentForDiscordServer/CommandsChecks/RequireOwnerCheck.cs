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

using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;

namespace MADS.CommandsChecks;

public class RequireOwnerCheck : IContextCheck<RequireOwnerAttribute>
{
    public ValueTask<string?> ExecuteCheckAsync(RequireOwnerAttribute attribute, CommandContext context)
    {
        DiscordApplication app = context.Client.CurrentApplication;
        DiscordUser me = context.Client.CurrentUser;
        
        bool isOwner = app is not null ? app!.Owners!.Any(x => x.Id == context.User.Id) : context.User.Id == me.Id;
        
        if (!isOwner) 
        {
            return ValueTask.FromResult<string?>("User must be on of the application owner");
        }
        
        return ValueTask.FromResult<string?>(null);
    }
}