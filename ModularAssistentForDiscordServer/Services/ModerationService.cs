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

using DSharpPlus.Entities;
using MADS.Entities;
using MADS.Extensions;
using Microsoft.Extensions.Caching.Memory;

namespace MADS.Services;

internal class ModerationService
{
    private List<string> _messageKeys;
    private IMemoryCache _cache;

    private MemoryCacheEntryOptions _options;


    internal ModerationService(IMemoryCache cache)
    {
        _cache = cache;
        _messageKeys = new List<string>();

        _options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
            .RegisterPostEvictionCallback(PostEvictionCallback);
    }

    public async Task HandleMessage(DiscordMessage message)
    {
        if (message.Channel.Guild is null) return;
        if (message.Author.IsBot) return;
        if (message.WebhookMessage) return;
        var key = CreateMessageKey(message);
        var cachedRecord = message.ToCached();

        var guildKey = CreateMessageKey(message.Channel.Guild.Id);
        var messagesInGuild = _messageKeys
            .Where(x => x.StartsWith(guildKey))
            .Select(x => _cache.Get<CachedMessage>(x))
            .Where(x => x.Content.Equals(cachedRecord.Content))
            .ToList();

        var channelKey = CreateMessageKey(message.Channel.Guild.Id, message.ChannelId);
        var messagesInChannel = _messageKeys
            .Where(x => x.StartsWith(channelKey))
            .Select(x => _cache.Get<CachedMessage>(x))
            .Where(x => x.Content.Equals(cachedRecord.Content))
            .ToList();

        if (messagesInGuild.Count() > 2)
        {
            await message.DeleteAsync();
            return;
        }
        
        
    }

    public void HandleMessageDeletion(DiscordMessage message)
    {
        var key = CreateMessageKey(message);
        _messageKeys.Remove(key);
        _cache.Remove(key);
    }

    private void PostEvictionCallback(object key, object value, EvictionReason reason, object state)
    {
        _messageKeys.Remove((string) key);
    }

    private string CreateMessageKey(DiscordMessage message)
    {
        return $"mod:message:{message.Channel.Guild}:{message.Channel}:{message.Id}";
    }

    private string CreateMessageKey(ulong guild)
    {
        return $"mod:message:{guild}";
    }

    private string CreateMessageKey(ulong guild, ulong channel)
    {
        return $"mod:message:{guild}:{channel}";
    }
}