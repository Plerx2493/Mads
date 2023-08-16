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
using MADS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MADS.Commands.AutoCompletion;

public class VoiceAlertAutoCompletion : IAutocompleteProvider
{
    private VoiceAlertService _voiceAlertService;
    
    public VoiceAlertAutoCompletion(IServiceProvider services)
    {
        _voiceAlertService = services.GetRequiredService<VoiceAlertService>();
    }
    
    public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
    {
        var choices = await _voiceAlertService.GetVoiceAlerts(ctx.User.Id);

        var result = new List<DiscordAutoCompleteChoice>();
        
        foreach (var choice in choices)
        {
            var chn = ctx.Guild.GetChannel(choice.ChannelId);
            result.Add(new DiscordAutoCompleteChoice(chn.Name, choice.ChannelId.ToString()));
        }
        
        return result;
    }
}