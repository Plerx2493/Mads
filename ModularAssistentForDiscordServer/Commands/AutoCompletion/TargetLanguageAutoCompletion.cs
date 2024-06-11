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

using DeepL;
using DeepL.Model;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

namespace MADS.Commands.AutoCompletion;

public class TargetLanguageAutoCompletion : IAutoCompleteProvider
{
    private readonly Translator _translator;
    
    public TargetLanguageAutoCompletion(Translator translator)
    {
        _translator = translator;
    }
    
    public async ValueTask<IReadOnlyDictionary<string, object>> AutoCompleteAsync(AutoCompleteContext context)
    {
        TargetLanguage[] sourceLangs = await _translator.GetTargetLanguagesAsync();
        Dictionary<string, object> choices = sourceLangs
            .Where(x => x.ToString().StartsWith(context.UserInput.ToString() ?? ""))
            .Take(25)
            .Select(x => x.ToString())
            .ToDictionary(x => x, x => (object)x);
        
        return choices;
    }
}