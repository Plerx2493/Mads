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