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

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MADS.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MADS.Commands.AutoCompletion;

public class ReminderAutoCompletion : IAutocompleteProvider
{
    private readonly IDbContextFactory<MadsContext> _factory;
    
    public ReminderAutoCompletion(IServiceProvider services)
    {
        _factory = services.GetRequiredService<IDbContextFactory<MadsContext>>();
    }
    
    public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
    {
        await using MadsContext db = await _factory.CreateDbContextAsync();
        IEnumerable<DiscordAutoCompleteChoice> choices = db.Reminders
            .Where(x => x.UserId == ctx.User.Id)
            .Select(x => x.Id.ToString())
            .Take(25)
            .Select(x => new DiscordAutoCompleteChoice(x, x))
            .ToList();
        
        return choices;
    }
}