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