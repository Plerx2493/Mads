using DSharpPlus;
using DSharpPlus.Entities;
using MADS.Entities;
using MADS.JsonModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MADS.Extensions;

public static class ExtensionMethods
{
    public static IServiceCollection AddDbFactoryDebugOrRelease(this IServiceCollection serviceCollection,
        ConfigJson config)
    {
        serviceCollection.AddDbContextFactory<MadsContext>(
            options => options.UseMySql(config.ConnectionString, ServerVersion.AutoDetect(config.ConnectionString))
        );

        return serviceCollection;
    }

    public static async Task<DiscordEmbed> GetEmbedAsync(this QuoteDbEntity quote, DiscordClient client)
    {
        var quotedUser = await client.GetUserAsync(quote.QuotedUserId);
        var user = await client.GetUserAsync(quote.UserId);

        var embed = new DiscordEmbedBuilder()
            .WithAuthor(quotedUser.Username, quotedUser.AvatarUrl, quotedUser.AvatarUrl)
            .WithTitle($"Said following {Formatter.Timestamp(quote.CreatedAt)}:")
            .WithDescription(quote.Content)
            .WithFooter("Quoted by " + user.Username);

        return embed;
    }

    public static  DiscordMessageBuilder GetMessage(this ReminderDbEntity reminder)
    {
        var message = new DiscordMessageBuilder();
        if(reminder.MentionedMessage != 0) message.WithReply(reminder.MentionedMessage);
        
        
        var embed = new DiscordEmbedBuilder();
        
        embed.WithTitle(Formatter.Timestamp(reminder.CreationTime) + "you wanted to be reminded:")
             .WithDescription(reminder.ReminderText)
             .WithColor(DiscordColor.Green);
        
        message.WithEmbed(embed);
        
        return message;
    }
}