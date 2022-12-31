using System.Collections.Concurrent;
using DSharpPlus.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace MADS.Services;

public class VolatileMemoryService
{
    public MessageSnipe MessageSnipe;
    public VoiceTroll VoiceTroll;
    
    public VolatileMemoryService()
    {
        VoiceTroll = new VoiceTroll();
        MessageSnipe = new MessageSnipe();
    }
    
}

public class MessageSnipe
{
    private MemoryCache _cachedMessages;

    public MessageSnipe()
    {
        var options = new MemoryCacheOptions()
        {
            ExpirationScanFrequency = TimeSpan.FromMinutes(5)
        };
        _cachedMessages = new(options);
    }

    public void AddMessage(DiscordMessage message)
    {
        
    }
    
    public void DeleteMessage(DiscordMessage message)
    {
        
    }
    
    public void DeleteMessageFromChannel(ulong message)
    {
        
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