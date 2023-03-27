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

namespace MADS.Commands;

public class CommandUtility
{
    public static DiscordEmbedBuilder GetDiscordEmbed()
    {
        var standardEmbed = new DiscordEmbedBuilder
        {
            Color = new Optional<DiscordColor>(new DiscordColor(0, 255, 194))
        };
        return standardEmbed;
    }
}