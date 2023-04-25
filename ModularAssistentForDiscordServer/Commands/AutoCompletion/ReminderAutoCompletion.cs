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

using System.Collections;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MADS.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MADS.Commands.AutoCompletion;

public class ReminderAutoCompletion : IAutocompleteProvider
{
    public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
    {
        var factory = ctx.Services.GetRequiredService<IDbContextFactory<MadsContext>>();
        await using var db = await factory.CreateDbContextAsync();
        var choices = db.Reminders
                        .Where(x => x.UserId == ctx.User.Id)
                        .Select(x => new DiscordAutoCompleteChoice(x.Id.ToString(), x.Id.ToString()))
                        .ToList();
        return choices;
    }
}