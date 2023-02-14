using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MADS.Entities;
using MADS.Extensions;
using Microsoft.EntityFrameworkCore;

namespace MADS.Commands.Slash;

[SlashCommandGroup("Quotes", "Commands related to adding and retrieving quotes")]
public class Quotes : MadsBaseApplicationCommand
{
    public IDbContextFactory<MadsContext> ContextFactory { get; set; }

    [SlashCommand("add", "Add a quote form a user")]
    public async Task AddQuoteUser
    (
        InteractionContext ctx,
        [Option("User", "User who is quoted")] DiscordUser user,
        [Option("Content", "Quoted content")] string content
    )
    {
        await ctx.DeferAsync(true);
        using var db = await ContextFactory.CreateDbContextAsync();

        var newQuote = new QuoteDbEntity()
        {
            Content = content,
            CreatedAt = DateTime.Now,
            DiscordGuildId = ctx.Guild.Id,
            QuotedUserId = user.Id,
            UserId = ctx.User.Id
        };


        await db.Quotes.AddAsync(newQuote);
        await db.SaveChangesAsync();
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Success"));

        await IntendedWait(5000);
        await ctx.DeleteResponseAsync();
    }
}