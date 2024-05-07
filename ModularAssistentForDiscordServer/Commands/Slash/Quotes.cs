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
using DSharpPlus.Entities;
using MADS.Entities;
using MADS.Extensions;
using MADS.Services;

namespace MADS.Commands.Slash;

[Command("Quotes"), Description("Commands related to adding and retrieving quotes"), RequireGuild]
public sealed class Quotes
{
    private readonly QuotesService _quotesService;

    public Quotes(QuotesService quotesService)
    {
        _quotesService = quotesService;
    }
    
    [Command("add"), Description("Add a quote form a user")]
    public async Task AddQuoteUser
    (
        CommandContext ctx,
        [Description("User who is quoted")] DiscordUser user,
        [Description("Quoted content")] string content
    )
    {
        await ctx.DeferAsync(true);


        QuoteDbEntity newQuote = new()
        {
            Content = content,
            CreatedAt = DateTime.Now,
            DiscordGuildId = ctx.Guild!.Id,
            QuotedUserId = user.Id,
            UserId = ctx.User.Id
        };


        _quotesService.AddQuote(newQuote);

        DiscordEmbed embed = await newQuote.GetEmbedAsync(ctx.Client);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }

    [Command("getRandom"), Description("Get a random quote from this server")]
    public async Task GetRndQuote
    (
        CommandContext ctx
    )
    {
        await ctx.DeferAsync(true);

        QuoteDbEntity quote = await _quotesService.GetRndGuildAsync(ctx.Guild!.Id);

        DiscordEmbed embed = await quote.GetEmbedAsync(ctx.Client);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }
}