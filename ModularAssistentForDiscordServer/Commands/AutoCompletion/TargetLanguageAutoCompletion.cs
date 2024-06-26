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

using DeepL.Model;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using MADS.Services;

namespace MADS.Commands.AutoCompletion;

public class TargetLanguageAutoCompletion : IAutoCompleteProvider
{
    private readonly TranslateInformationService _service;
    
    public TargetLanguageAutoCompletion(TranslateInformationService service)
    {
        _service = service;
    }
    
    public async ValueTask<IReadOnlyDictionary<string, object>> AutoCompleteAsync(AutoCompleteContext context)
    {
        TargetLanguage[] sourceLangs = await _service.GetTargetLanguagesAsync();
        Dictionary<string, object> choices = sourceLangs
            .Where(x => x.ToString().ToLowerInvariant().StartsWith(context.UserInput.ToLowerInvariant()))
            .Take(25)
            .ToDictionary(x => x.ToString(), x => (object)x.Code);
        
        return choices;
    }
}