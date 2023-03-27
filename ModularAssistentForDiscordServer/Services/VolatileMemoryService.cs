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
using DSharpPlus.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MADS.Services;

public class VolatileMemoryService
{
    public readonly MessageSnipe MessageSnipe;
    public readonly VoiceTroll   VoiceTroll;

    public VolatileMemoryService(IMemoryCache memoryCache, DiscordClient client)
    {
        VoiceTroll = new VoiceTroll();
        MessageSnipe = new MessageSnipe(memoryCache, client);
    }
}

public class MessageSnipe
{
    private readonly IMemoryCache            _memoryCache;
    private readonly DiscordClient           _client;
    private readonly MemoryCacheEntryOptions _options;

    public MessageSnipe(IMemoryCache memoryCache, DiscordClient client)
    {
        _memoryCache = memoryCache;
        _client = client;
        _options = new MemoryCacheEntryOptions();
        _options.SetAbsoluteExpiration(TimeSpan.FromHours(12))
                .SetSize(1)
                .RegisterPostEvictionCallback(PostEvictionCallback);
    }

    private void PostEvictionCallback(object key, object value, EvictionReason reason, object state)
    {
        _client.Logger.LogTrace($"MessageSniper: Message eviction - {reason}");
    }

    public void AddMessage(DiscordMessage message)
    {
        var id = CreateSnipedGuid(message.ChannelId);
        _memoryCache.Remove(id);
        _memoryCache.Set(id, message, _options);
    }

    public void DeleteMessage(ulong channel)
    {
        var id = CreateSnipedGuid(channel);
        _memoryCache.Remove(id);
    }

    public void AddEditedMessage(DiscordMessage message)
    {
        var id = CreateSnipedEditedGuid(message.ChannelId);
        _memoryCache.Remove(id);
        _memoryCache.Set(id, message, _options);
    }

    public void DeleteEditedMessage(ulong channel)
    {
        var id = CreateSnipedEditedGuid(channel);
        _memoryCache.Remove(id);
    }

    public bool TryGetMessage(ulong channelId, out DiscordMessage message)
    {
        var id = CreateSnipedGuid(channelId);
        var result = _memoryCache.TryGetValue(id, out message);
        if (result) _memoryCache.Remove(id);
        return result;
    }

    public bool TryGetEditedMessage(ulong channelId, out DiscordMessage message)
    {
        var id = CreateSnipedEditedGuid(channelId);
        var result = _memoryCache.TryGetValue(id, out message);
        if (result) _memoryCache.Remove(id);
        return result;
    }

    private static string CreateSnipedGuid(ulong channelId)
    {
        return $"snipedMessage_{channelId}";
    }

    private static string CreateSnipedEditedGuid(ulong channelId)
    {
        return $"snipedMessage_edit_{channelId}";
    }
}

public class VoiceTroll
{
    private readonly List<ulong> _voiceTrollUser;

    public VoiceTroll()
    {
        _voiceTrollUser = new List<ulong>();
    }

    public void Add(DiscordUser user)
    {
        if (!_voiceTrollUser.Contains(user.Id)) _voiceTrollUser.Add(user.Id);
    }

    public void Delete(DiscordUser user)
    {
        _voiceTrollUser.RemoveAll(x => user.Id == x);
    }

    public bool Active(DiscordUser user)
    {
        return _voiceTrollUser.Contains(user.Id);
    }
}