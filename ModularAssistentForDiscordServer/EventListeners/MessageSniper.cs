using DSharpPlus;
using DSharpPlus.EventArgs;
using MADS.Services;
using Microsoft.Extensions.Logging;

namespace MADS.EventListeners;

internal static partial class EventListener
{
    public static void EnableMessageSniper(DiscordClient client, VolatileMemoryService memory)
    {
        client.MessageDeleted += (sender, args) =>
        {
            var _ = MessageSniperDeleted(sender, args, memory);
            return Task.CompletedTask;
        };

        client.MessageUpdated += (sender, args) =>
        {
            var _ = MessageSniperEdited(sender, args, memory);
            return Task.CompletedTask;
        };
    }

    private static Task MessageSniperDeleted
    (
        DiscordClient sender,
        MessageDeleteEventArgs e,
        VolatileMemoryService memory
    )
    {
        if (e.Message == null) return Task.CompletedTask;
        if (e.Message.WebhookMessage) return Task.CompletedTask;

        if ((!string.IsNullOrEmpty(e.Message?.Content) || e.Message?.Attachments.Count > 0) && !e.Message.Author.IsBot)
        {
            memory.MessageSnipe.AddMessage(e.Message);

            sender.Logger.LogTrace("Message added to cache");
        }
        return Task.CompletedTask;
    }

    private static Task MessageSniperEdited
    (
        DiscordClient sender,
        MessageUpdateEventArgs e,
        VolatileMemoryService memory
    )
    {
        if (e.Message == null) return Task.CompletedTask;
        if (e.Message.WebhookMessage) return Task.CompletedTask;

        if (string.IsNullOrEmpty(e.MessageBefore?.Content) && !(e.MessageBefore?.Attachments.Count > 0)
            || e.Message.Author.IsBot) return Task.CompletedTask;

        memory.MessageSnipe.AddEditedMessage(e.MessageBefore);

        sender.Logger.LogTrace("Message edit added to cache");
        return Task.CompletedTask;
    }
}