using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using MADS.Entities;
using MADS.Extensions;
using MADS.Services;

namespace MADS.Commands.Slash;

[SlashCommandGroup("Quotes", "Commands related to adding and retrieving quotes"), SlashRequireGuild]
public class Quotes : MadsBaseApplicationCommand
{
    public QuotesService QuotesService { get; set; }

    [SlashCommand("add", "Add a quote form a user")]
    public async Task AddQuoteUser
    (
        InteractionContext ctx,
        [Option("User", "User who is quoted")] DiscordUser user,
        [Option("Content", "Quoted content")] string content
    )
    {
        await ctx.DeferAsync(true);


        var newQuote = new QuoteDbEntity()
        {
            Content = content,
            CreatedAt = DateTime.Now,
            DiscordGuildId = ctx.Guild.Id,
            QuotedUserId = user.Id,
            UserId = ctx.User.Id
        };


        QuotesService.AddQuote(newQuote);

        var embed = await newQuote.GetEmbedAsync(ctx.Client);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }

    [SlashCommand("getRandom", "Get a random quote from this server")]
    public async Task GetRndQuote
    (
        InteractionContext ctx
    )
    {
        await ctx.DeferAsync(true);

        var quote = await QuotesService.GetRndGuildAsync(ctx.Guild.Id);

        var embed = await quote.GetEmbedAsync(ctx.Client);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }
}