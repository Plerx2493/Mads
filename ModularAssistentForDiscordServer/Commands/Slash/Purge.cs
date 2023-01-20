using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using MADS.Extensions;

namespace MADS.Commands.Slash;

public class Purge : MadsBaseApplicationCommand
{
    [SlashCommand("purge", "Purges messages"),
     SlashRequirePermissions(Permissions.ManageMessages),
     SlashRequireGuild]
    public async Task PurgeMessages
        (InteractionContext ctx, [Option("amount", "Delete a bunch of messages")] long amount = 100)
    {
        if (amount > 100)
        {
            await ctx.CreateResponseAsync("You cannot purge more than 100 messages at once", true);
            return;
        }

        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder());
        var response = await ctx.GetOriginalResponseAsync();

        var messagesApi = await ctx.Channel.GetMessagesAsync((int)amount);
        List<DiscordMessage> messages = new();
        messages.AddRange(messagesApi);

        messages.RemoveAll(x => (DateTime.UtcNow - x.Timestamp).TotalDays >= 14);
        messages.Remove(response);

        await ctx.Channel.DeleteMessagesAsync(messages);
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"{messages.Count} messages deleted"));

        await IntendedWait(10_000);

        await ctx.DeleteResponseAsync();
    }
}