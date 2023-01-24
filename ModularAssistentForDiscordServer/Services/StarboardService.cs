#nullable enable
using DSharpPlus;
using DSharpPlus.EventArgs;
using MADS.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CancellationToken = System.Threading.CancellationToken;

namespace MADS.Services;

public class StarboardService : IHostedService
{
    private readonly DiscordClient                     _client;
    private readonly Queue<DiscordReactionUpdateEvent> _messageQueue;
    private          bool                              _active;
    private readonly IDbContextFactory<MadsContext>    _dbFactory;

    public StarboardService(DiscordClient client, IDbContextFactory<MadsContext> dbFactory)
    {
        _client = client;
        _messageQueue = new Queue<DiscordReactionUpdateEvent>();
        _active = false;
        _dbFactory = dbFactory;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _client.MessageReactionAdded += HandleReactionAdded;
        _client.MessageReactionRemoved += HandleReactionRemoved;
        _client.MessageReactionsCleared += HandleReactionsCleared;
        _client.MessageReactionRemovedEmoji += HandleReactionEmojiRemoved;
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _client.MessageReactionAdded -= HandleReactionAdded;
        _client.MessageReactionRemoved -= HandleReactionRemoved;
        _client.MessageReactionsCleared -= HandleReactionsCleared;
        _client.MessageReactionRemovedEmoji -= HandleReactionEmojiRemoved;
        
        return Task.CompletedTask;
    }

    private Task HandleReactionAdded(DiscordClient s, MessageReactionAddEventArgs e)
    {
        var msg = new DiscordReactionUpdateEvent
        {
            Message = e.Message,
            EventArgs = e,
            Type = DiscordReactionUpdateType.ReactionAdded
        };
        
        _messageQueue.Enqueue(msg);
        var _ = HandleQueue();
        return Task.CompletedTask;
    }
    
    private Task HandleReactionRemoved(DiscordClient s, MessageReactionRemoveEventArgs e)
    {
        var msg = new DiscordReactionUpdateEvent
        {
            Message = e.Message,
            EventArgs = e,
            Type = DiscordReactionUpdateType.ReactionRemoved
        };
        
        _messageQueue.Enqueue(msg);
        var _ = HandleQueue();
        return Task.CompletedTask;
    }
    
    private Task HandleReactionsCleared(DiscordClient s, MessageReactionsClearEventArgs e)
    {
        var msg = new DiscordReactionUpdateEvent
        {
            Message = e.Message,
            EventArgs = e,
            Type = DiscordReactionUpdateType.ReactionsCleard
        };
        
        _messageQueue.Enqueue(msg);
        var _ = HandleQueue();
        return Task.CompletedTask;
    }
    
    private Task HandleReactionEmojiRemoved(DiscordClient s, MessageReactionRemoveEmojiEventArgs e)
    {
        var msg = new DiscordReactionUpdateEvent
        {
            Message = e.Message,
            EventArgs = e,
            Type = DiscordReactionUpdateType.ReactionEmojiRemoved
        };
        
        _messageQueue.Enqueue(msg);
        var _ = HandleQueue();
        return Task.CompletedTask;
    }

    private async Task HandleQueue()
    {
        if (_active) return;
        _active = true;
        
        while (_messageQueue.Any())
        {
            var msg = _messageQueue.Dequeue();
            await HandleEvent(msg);
        }

        _active = false;
    }

    private async Task HandleEvent(DiscordReactionUpdateEvent e)
    {
        if(!e.Message.Channel.GuildId.HasValue) return;

        if (e.Type == DiscordReactionUpdateType.ReactionAdded)
        {
            var eventArgs = (MessageReactionAddEventArgs)e.EventArgs;
            if (eventArgs.User.IsBot) return;
        }


        var db = await _dbFactory.CreateDbContextAsync();
        
        var guildSettings = db.Guilds.First(x => x.DiscordId == e.Message.Channel.GuildId).Settings;
        
        if (!guildSettings.StarboardActive) return;

        if (guildSettings.StarboardEmojiId == null 
            || guildSettings.StarboardThreshold == null
            || guildSettings.StarboardChannelId == null)
        {
            _client.Logger.LogError("GuildSettings incomplete: StarboardSettings");
            return;
        }

        switch (e.Type)
        {
            case DiscordReactionUpdateType.ReactionAdded:
            {
                var eventArgs = (MessageReactionAddEventArgs)e.EventArgs;
                if (eventArgs.Emoji.Id != guildSettings.StarboardEmojiId.Value) return;
                break;
            }

            case DiscordReactionUpdateType.ReactionRemoved:
            {
                var eventArgs = (MessageReactionRemoveEventArgs)e.EventArgs;
                if (eventArgs.Emoji.Id != guildSettings.StarboardEmojiId.Value) return;
                break;
            }

            case DiscordReactionUpdateType.ReactionsCleard:
                var msgs = db.Starboard.Where(x => x.DiscordMessageId == e.Message.Id && x.DiscordChannelId == e.Message.ChannelId);
                db.Starboard.RemoveRange(msgs);
                await db.SaveChangesAsync();
                return;

            case DiscordReactionUpdateType.ReactionEmojiRemoved:
                MessageReactionRemoveEmojiEventArgs argss = (MessageReactionRemoveEmojiEventArgs) e.EventArgs;
                if (argss.Emoji.Id != guildSettings.StarboardEmojiId.Value)
                {
                    return;
                }
                var msgss = db.Starboard.Where(x => x.DiscordMessageId == e.Message.Id && x.DiscordChannelId == e.Message.ChannelId);
                db.Starboard.RemoveRange(msgss);
                return;

            default:
                throw new ArgumentOutOfRangeException();
        }


        StarboardMessageDbEntity? starData;
        bool isNew;
        try
        {
            starData = db.Starboard.First(
                x => x.DiscordMessageId == e.Message.Id && x.DiscordChannelId == e.Message.ChannelId
            );
            isNew = false;
        }
        catch (Exception)
        {
            isNew = true;
        }

        if (isNew)
        {
            starData = new StarboardMessageDbEntity()
            {
                DiscordChannelId = e.Message.ChannelId,
                DiscordMessageId = e.Message.Id,
                DiscordGuildId = e.Message.Channel.Guild.Id,
                Stars = 1
            };
        }
        
        
        
    }
}