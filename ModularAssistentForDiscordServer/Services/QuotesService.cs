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

using MADS.Entities;
using Microsoft.EntityFrameworkCore;

namespace MADS.Services;

public class QuotesService
{
    private readonly IDbContextFactory<MadsContext> _dbContextFactory;

    public QuotesService(IDbContextFactory<MadsContext> factory)
    {
        _dbContextFactory = factory;
    }

    public async Task<List<QuoteDbEntity>> GetQuotesGuildAsync(ulong guildId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return db.Quotes.Where(x => x.DiscordGuildId == guildId).ToList();
    }

    public async Task<QuoteDbEntity> GetRndGuildAsync(ulong guildId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var rnd = new Random();
        var quotes = db.Quotes.Where(x => x.DiscordGuildId == guildId).ToArray();

        return quotes[rnd.Next(0, quotes.Length)];
    }

    public async void AddQuote(QuoteDbEntity quote)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        db.Quotes.Add(quote);
        await db.SaveChangesAsync();
    }

    public async void DeleteQuotesById(ulong id)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var entities = db.Quotes.Where(x => x.Id == id).ToList();
        db.Quotes.RemoveRange(entities);
        await db.SaveChangesAsync();
    }

    public async void DeleteQuotesByUser(ulong id)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var entities = db.Quotes.Where(x => x.QuotedUserId == id).ToList();
        db.Quotes.RemoveRange(entities);
        await db.SaveChangesAsync();
    }

    public async void DeleteQuotesByCreator(ulong id)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var entities = db.Quotes.Where(x => x.UserId == id).ToList();
        db.Quotes.RemoveRange(entities);
        await db.SaveChangesAsync();
    }

    public async void DeleteQuotesByGuild(ulong id)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var entities = db.Quotes.Where(x => x.DiscordGuildId == id).ToList();
        db.Quotes.RemoveRange(entities);
        await db.SaveChangesAsync();
    }
}