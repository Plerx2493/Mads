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

        if ((string.IsNullOrEmpty(e.MessageBefore?.Content) && !(e.MessageBefore?.Attachments.Count > 0))
            || e.Message.Author.IsBot) return Task.CompletedTask;

        memory.MessageSnipe.AddEditedMessage(e.MessageBefore);

        sender.Logger.LogTrace("Message edit added to cache");
        return Task.CompletedTask;
    }
}