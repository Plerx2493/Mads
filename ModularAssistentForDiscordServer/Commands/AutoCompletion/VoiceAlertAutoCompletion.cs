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

using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using MADS.Entities;
using MADS.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MADS.Commands.AutoCompletion;

public class VoiceAlertAutoCompletion : IAutoCompleteProvider
{
    private readonly VoiceAlertService _voiceAlertService;
    
    public VoiceAlertAutoCompletion(IServiceProvider services)
    {
        _voiceAlertService = services.GetRequiredService<VoiceAlertService>();
    }
    
    public async ValueTask<IReadOnlyDictionary<string, object>> AutoCompleteAsync(AutoCompleteContext context)
    {
        IEnumerable<VoiceAlert> choices = await _voiceAlertService.GetVoiceAlerts(context.User.Id);
        
        List<DiscordChannel> result = new();
        foreach (VoiceAlert choice in choices)
        {
            DiscordChannel channel = await context.Client.GetChannelAsync(choice.ChannelId);
            result.Add(channel);
        }
        
        Dictionary<string, object> dict = result
            .Where(x => x.Name.StartsWith(context.UserInput))
            .Take(25)
            .ToDictionary(x => x.Name, x => (object) x.Id);
        
        return dict;
    }
}