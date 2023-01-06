using DSharpPlus.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace MADS.Services;

public class VolatileMemoryService
{
    public readonly MessageSnipe MessageSnipe;
    public readonly VoiceTroll   VoiceTroll;

    public VolatileMemoryService(IMemoryCache memoryCache)
    {
        VoiceTroll = new VoiceTroll();
        MessageSnipe = new MessageSnipe(memoryCache);
    }
}

public class MessageSnipe
{
    private readonly IMemoryCache _cachedMessages;

    public MessageSnipe(IMemoryCache memoryCache)
    {
        _cachedMessages = memoryCache;
    }

    public void AddMessage(DiscordMessage message)
    {
        var id = CreateSnipedGuid(message.ChannelId);
        _cachedMessages.Set(id, message, TimeSpan.FromHours(12));
    }

    public void DeleteMessage(ulong channel)
    {
        var id = CreateSnipedGuid(channel);
        _cachedMessages.Remove(id);
    }

    public void AddEditedMessage(DiscordMessage message)
    {
        var id = CreateSnipedEditedGuid(message.ChannelId);
        _cachedMessages.Set(id, message, TimeSpan.FromHours(12));
    }

    public void DeleteEditedMessage(ulong channel)
    {
        var id = CreateSnipedEditedGuid(channel);
        _cachedMessages.Remove(id);
    }

    public bool TryGetMessage(ulong channelId, out DiscordMessage message)
    {
        var id = CreateSnipedGuid(channelId);
        var result = _cachedMessages.TryGetValue(id, out message);
        if (result) _cachedMessages.Remove(id);
        return result;
    }

    public bool TryGetEditedMessage(ulong channelId, out DiscordMessage message)
    {
        var id = CreateSnipedEditedGuid(channelId);
        var result = _cachedMessages.TryGetValue(id, out message);
        if (result) _cachedMessages.Remove(id);
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