using DSharpPlus;
using DSharpPlus.EventArgs;
using MADS.Services;

namespace MADS.EventListeners;

internal static partial class EventListener
{
    public static void EnableMessageSniper(DiscordClient client, VolatileMemoryService memory)
    {
        client.MessageDeleted += async (sender, args) =>
        {
            MessageSniperDeleted(sender,args,memory);
        };
        
        client.MessageUpdated += async (sender, args) =>
        {
            MessageSniperEdited(sender,args,memory);
        };
    }

    private static async void MessageSniperDeleted(
            DiscordClient sender, MessageDeleteEventArgs e, VolatileMemoryService memory)
    {
        if (e.Message == null) return;
        if (e.Message.WebhookMessage) return;
        
        if (((!string.IsNullOrEmpty(e.Message?.Content)) || e.Message?.Attachments.Count > 0) && !e.Message.Author.IsBot)
        {
            memory.MessageSnipe.AddMessage(e.Message);
            
            Console.WriteLine("Added to cache");
        }
    }
    
    private static async void MessageSniperEdited(
        DiscordClient sender, MessageUpdateEventArgs e, VolatileMemoryService memory)
    {
        if (e.Message == null) return;
        if (e.Message.WebhookMessage) return;

        if (((string.IsNullOrEmpty(e.MessageBefore?.Content)) && !(e.MessageBefore?.Attachments.Count > 0))
            || e.Message.Author.IsBot) return;
        
        memory.MessageSnipe.AddEditedMessage(e.MessageBefore);
            
        Console.WriteLine("Added to cache (edited)");
    }
}