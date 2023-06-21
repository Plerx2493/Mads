﻿// Copyright 2023 Plerx2493
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
using DSharpPlus.EventArgs;
using Humanizer;
using MADS.Extensions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MADS.Services;

public class MessageSnipeService : IHostedService
{
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _options;
    private readonly DiscordClientService _discordClientService;
    private DiscordClient Discord  => _discordClientService.DiscordClient;

    public MessageSnipeService(IMemoryCache memoryCache, DiscordClientService discordClientService)
    {
        _memoryCache = memoryCache;
        _discordClientService = discordClientService;
        _options = new MemoryCacheEntryOptions();
        _options.SetAbsoluteExpiration(TimeSpan.FromHours(12))
            .SetSize(1)
            .RegisterPostEvictionCallback(PostEvictionCallback);
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Log.Warning("Sniper active!");
        Discord.MessageDeleted += MessageSniperDeleted;
        Discord.MessageUpdated += MessageSniperEdited;
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Discord.MessageDeleted -= MessageSniperDeleted;
        Discord.MessageUpdated -= MessageSniperEdited;
        
        return Task.CompletedTask;
    }

    private Task MessageSniperDeleted
    (
        DiscordClient sender,
        MessageDeleteEventArgs e
    )
    {
        if (e.Message == null) return Task.CompletedTask;
        if (e.Message.WebhookMessage) return Task.CompletedTask;

        if ((!string.IsNullOrEmpty(e.Message?.Content) || e.Message?.Attachments.Count > 0) && !e.Message.Author.IsBot)
        {
            AddMessage(e.Message);
            Log.Warning("Sniped!");
            sender.Logger.LogTrace("Message added to cache");
        }

        return Task.CompletedTask;
    }

    private Task MessageSniperEdited
    (
        DiscordClient sender,
        MessageUpdateEventArgs e
    )
    {
        if (e.Message == null) return Task.CompletedTask;
        if (e.Message.WebhookMessage) return Task.CompletedTask;

        if ((string.IsNullOrEmpty(e.MessageBefore?.Content) && !(e.MessageBefore?.Attachments.Count > 0))
            || e.Message.Author.IsBot) return Task.CompletedTask;

        AddEditedMessage(e.MessageBefore);
        Log.Warning("Sniped!");
        
        sender.Logger.LogTrace("Message edit added to cache");
        return Task.CompletedTask;
    }

    private static void PostEvictionCallback(object key, object value, EvictionReason reason, object state)
    {
        Log.ForContext<MessageSnipeService>().Verbose("MessageSniper: Message eviction - {Reason}", reason.Humanize());
    }

    public void AddMessage(DiscordMessage message)
    {
        var id = CacheHelper.GetMessageSnipeKey(message.ChannelId);
        _memoryCache.Remove(id);
        _memoryCache.Set(id, message, _options);
    }

    public void AddEditedMessage(DiscordMessage message)
    {
        var id = CacheHelper.GetMessageEditSnipeKey(message.ChannelId);
        _memoryCache.Remove(id);
        _memoryCache.Set(id, message, _options);
    }

    public void DeleteEditedMessage(ulong channel)
    {
        var id = CacheHelper.GetMessageEditSnipeKey(channel);
        _memoryCache.Remove(id);
    }
    
    public void DeleteMessage(ulong channel)
    {
        var id = CacheHelper.GetMessageSnipeKey(channel);
        _memoryCache.Remove(id);
    }

    public bool TryGetMessage(ulong channelId, out DiscordMessage message)
    {
        var id = CacheHelper.GetMessageSnipeKey(channelId);
        var result = _memoryCache.TryGetValue(id, out message);
        if (result) _memoryCache.Remove(id);
        return result;
    }

    public bool TryGetEditedMessage(ulong channelId, out DiscordMessage message)
    {
        var id = CacheHelper.GetMessageEditSnipeKey(channelId);
        var result = _memoryCache.TryGetValue(id, out message);
        if (result) _memoryCache.Remove(id);
        return result;
    }
}