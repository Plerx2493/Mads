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
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;

namespace MADS.Extensions;

public static class ContextExtensions
{
    public static async ValueTask DeferAsync(this CommandContext ctx, bool ephemeral = false)
    {
        if (ctx is SlashCommandContext slashContext)
        {
            await slashContext.DeferResponseAsync(ephemeral);
            return;
        }

        await ctx.DeferResponseAsync();
    }

    public static async ValueTask RespondErrorAsync(this CommandContext ctx, string message, bool isEphemeral = false)
    {
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithTitle("Error")
            .WithDescription(message)
            .WithColor(DiscordColor.Red);

        DiscordInteractionResponseBuilder response = new DiscordInteractionResponseBuilder()
            .AddEmbed(embed)
            .AsEphemeral(isEphemeral);

        await ctx.RespondAsync(response);
    }

    public static async ValueTask RespondSuccessAsync(this CommandContext ctx, string message, bool isEphemeral = false)
    {
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithTitle("Success")
            .WithDescription(message)
            .WithColor(DiscordColor.Green);

        DiscordInteractionResponseBuilder response = new DiscordInteractionResponseBuilder()
            .AddEmbed(embed)
            .AsEphemeral(isEphemeral);

        await ctx.RespondAsync(response);
    }
}