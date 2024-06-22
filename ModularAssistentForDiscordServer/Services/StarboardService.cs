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

using System.Threading.Channels;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Humanizer;
using MADS.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Serilog;
using ILogger = Serilog.ILogger;

namespace MADS.Services;

public class StarboardService : IHostedService
{
    private readonly DiscordClient _client;
    private readonly IDbContextFactory<MadsContext> _dbFactory;
    private readonly Channel<DiscordReactionUpdateEvent> _messageChannel;
    private readonly CancellationTokenSource _cts = new();
    private Task? _handleQueueTask;
    private bool _stopped;
    
    private static readonly ILogger _logger = Log.ForContext<StarboardService>();
    
    public StarboardService(DiscordCommandService command, IDbContextFactory<MadsContext> dbFactory)
    {
        _client = command.DiscordClient;
        _messageChannel = Channel.CreateUnbounded<DiscordReactionUpdateEvent>();
        _dbFactory = dbFactory;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _client.MessageReactionAdded += HandleReactionAdded;
        _client.MessageReactionRemoved += HandleReactionRemoved;
        _client.MessageReactionsCleared += HandleReactionsCleared;
        _client.MessageReactionRemovedEmoji += HandleReactionEmojiRemoved;
        
        _handleQueueTask = Task.Factory.StartNew(HandleQueueAsync, _cts.Token, TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
        
        _logger.Information("Starboard active");
        return Task.CompletedTask;
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _client.MessageReactionAdded -= HandleReactionAdded;
        _client.MessageReactionRemoved -= HandleReactionRemoved;
        _client.MessageReactionsCleared -= HandleReactionsCleared;
        _client.MessageReactionRemovedEmoji -= HandleReactionEmojiRemoved;
        
        _stopped = true;
        _logger.Information("Starboard stopped");
        _cts.Cancel();
        _handleQueueTask?.Dispose();
        _handleQueueTask = null;
        return Task.CompletedTask;
    }
    
    private async Task HandleReactionAdded(DiscordClient s, MessageReactionAddedEventArgs e)
    {
        DiscordReactionUpdateEvent msg = new()
        {
            Message = e.Message,
            EventArgs = e,
            Type = DiscordReactionUpdateType.ReactionAdded
        };
        await _messageChannel.Writer.WriteAsync(msg);
    }
    
    private async Task HandleReactionRemoved(DiscordClient s, MessageReactionRemovedEventArgs e)
    {
        DiscordReactionUpdateEvent msg = new()
        {
            Message = e.Message,
            EventArgs = e,
            Type = DiscordReactionUpdateType.ReactionRemoved
        };
        await _messageChannel.Writer.WriteAsync(msg);
    }
    
    private async Task HandleReactionsCleared(DiscordClient s, MessageReactionsClearedEventArgs e)
    {
        DiscordReactionUpdateEvent msg = new()
        {
            Message = e.Message,
            EventArgs = e,
            Type = DiscordReactionUpdateType.ReactionsCleared
        };
        await _messageChannel.Writer.WriteAsync(msg);
    }
    
    private async Task HandleReactionEmojiRemoved(DiscordClient s, MessageReactionRemovedEmojiEventArgs e)
    {
        DiscordReactionUpdateEvent msg = new()
        {
            Message = e.Message,
            EventArgs = e,
            Type = DiscordReactionUpdateType.ReactionEmojiRemoved
        };
        await _messageChannel.Writer.WriteAsync(msg);
    }
    
    private async Task HandleQueueAsync()
    {
        while (!_stopped)
        {
            try
            {
                _logger.Verbose("StarboardService: waiting for message");
                DiscordReactionUpdateEvent msg = await _messageChannel.Reader.ReadAsync();
                _logger.Verbose("StarboardService: message received");
                await HandleEvent(msg);
            }
            catch (Exception e)
            {
                _logger.Error("Exception in {Source}: {Message}", nameof(StarboardService), e.Message);
            }
        }
    }
    
    private async Task HandleEvent(DiscordReactionUpdateEvent e)
    {
        if (e.Message.Channel is null)
        {
            return;
        }
        
        if (!e.Message.Channel?.GuildId.HasValue ?? false)
        {
            return;
        }
        
        if (e.Type == DiscordReactionUpdateType.ReactionAdded)
        {
            MessageReactionAddedEventArgs eventArgs = (MessageReactionAddedEventArgs)e.EventArgs;
            if (eventArgs.User.IsBot)
            {
                return;
            }
        }
        
        await using MadsContext db = await _dbFactory.CreateDbContextAsync();
        
        GuildConfigDbEntity? guildSettings =
            db.Configs.AsNoTracking().FirstOrDefault(x => x.GuildId == e.Message.Channel!.GuildId);
        
        if (guildSettings is null)
        {
            return;
        }
        
        if (!guildSettings.StarboardActive)
        {
            return;
        }
        
        if (guildSettings.StarboardEmojiId == null
            || guildSettings.StarboardThreshold == null
            || guildSettings.StarboardChannelId == null)
        {
            _logger.Error("GuildSettings incomplete: StarboardSettings");
            return;
        }
        
        DiscordEmoji discordEmoji;
        
        switch (e.Type)
        {
            case DiscordReactionUpdateType.ReactionAdded:
            {
                MessageReactionAddedEventArgs eventArgs = (MessageReactionAddedEventArgs)e.EventArgs;
                
                if (guildSettings.StarboardEmojiId != 0)
                {
                    discordEmoji = DiscordEmoji.FromGuildEmote(_client, guildSettings.StarboardEmojiId.Value);
                }
                else
                {
                    discordEmoji = DiscordEmoji.FromUnicode(_client, guildSettings.StarboardEmojiName!);
                }
                
                if (eventArgs.Emoji != discordEmoji)
                {
                    return;
                }
                
                break;
            }
            
            case DiscordReactionUpdateType.ReactionRemoved:
            {
                MessageReactionRemovedEventArgs eventArgs = (MessageReactionRemovedEventArgs)e.EventArgs;
                
                if (guildSettings.StarboardEmojiId != 0)
                {
                    discordEmoji = DiscordEmoji.FromGuildEmote(_client, guildSettings.StarboardEmojiId.Value);
                }
                else
                {
                    discordEmoji = DiscordEmoji.FromUnicode(_client, guildSettings.StarboardEmojiName!);
                }
                
                if (eventArgs.Emoji != discordEmoji)
                {
                    return;
                }
                
                break;
            }
            
            case DiscordReactionUpdateType.ReactionsCleared:
                IQueryable<StarboardMessageDbEntity> msgs = db.Starboard.Where(x =>
                    x.DiscordMessageId == e.Message.Id && x.DiscordChannelId == e.Message.ChannelId);
                db.Starboard.RemoveRange(msgs);
                await db.SaveChangesAsync();
                return;
            
            case DiscordReactionUpdateType.ReactionEmojiRemoved:
                MessageReactionRemovedEmojiEventArgs args = (MessageReactionRemovedEmojiEventArgs)e.EventArgs;
                
                if (guildSettings.StarboardEmojiId != 0)
                {
                    discordEmoji = DiscordEmoji.FromGuildEmote(_client, guildSettings.StarboardEmojiId.Value);
                }
                else
                {
                    discordEmoji = DiscordEmoji.FromUnicode(_client, guildSettings.StarboardEmojiName!);
                }
                
                if (args.Emoji != discordEmoji)
                {
                    return;
                }
                
                IQueryable<StarboardMessageDbEntity> messages = db.Starboard.Where(x =>
                    x.DiscordMessageId == e.Message.Id && x.DiscordChannelId == e.Message.ChannelId);
                db.Starboard.RemoveRange(messages);
                return;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        StarboardMessageDbEntity? starData;
        bool isNew = false;
        starData = db.Starboard.FirstOrDefault(x =>
            x.DiscordMessageId == e.Message.Id && x.DiscordChannelId == e.Message.ChannelId);
        
        if (starData is null)
        {
            starData = new StarboardMessageDbEntity
            {
                DiscordChannelId = e.Message.ChannelId,
                DiscordMessageId = e.Message.Id,
                DiscordGuildId = e.Message.Channel!.Guild.Id,
                Stars = 1
            };
            isNew = true;
        }
        
        if (e.Type is DiscordReactionUpdateType.ReactionAdded)
        {
            starData.Stars++;
        }
        else
        {
            starData.Stars--;
        }
        
        if (isNew)
        {
            db.Starboard.Add(starData);
        }
        else
        {
            db.Starboard.Update(starData);
        }
        
        await db.SaveChangesAsync();
        
        if (starData.Stars < guildSettings.StarboardThreshold)
        {
            if (isNew)
            {
                return;
            }
            
            if (starData.StarboardMessageId != 0)
            {
                await DeleteStarboardMessage(starData);
            }
            
            if (starData.Stars < 1 && !isNew)
            {
                db.Starboard.Remove(starData);
            }
            
            await db.SaveChangesAsync();
            return;
        }
        
        if (starData.StarboardMessageId == 0)
        {
            await CreateStarboardMessage(starData, guildSettings, discordEmoji);
            return;
        }
        
        await UpdateStarboardMessage(starData, discordEmoji);
    }
    
    private async Task DeleteStarboardMessage(StarboardMessageDbEntity starData)
    {
        DiscordMessage message;
        try
        {
            DiscordChannel channel = await _client.GetChannelAsync(starData.StarboardChannelId);
            message = await channel.GetMessageAsync(starData.StarboardMessageId);
        }
        catch (Exception)
        {
            _logger.Error("{Source}: Message with id {Id} not found", nameof(StarboardService),
                starData.DiscordMessageId);
            return;
        }
        
        await message.DeleteAsync();
        
        await using MadsContext db = await _dbFactory.CreateDbContextAsync();
        
        StarboardMessageDbEntity? starDataOld = db.Starboard.FirstOrDefault(
            x => x.DiscordMessageId == starData.DiscordMessageId && x.DiscordChannelId == starData.DiscordChannelId
        );
        
        if (starDataOld is null)
        {
            return;
        }
        
        starDataOld.StarboardMessageId = 0;
        starDataOld.StarboardChannelId = 0;
        starDataOld.StarboardGuildId = 0;
        
        db.Update(starDataOld);
        await db.SaveChangesAsync();
    }
    
    private async Task UpdateStarboardMessage(StarboardMessageDbEntity starData, DiscordEmoji emoji)
    {
        DiscordMessage message;
        try
        {
            DiscordChannel channel = await _client.GetChannelAsync(starData.StarboardChannelId);
            message = await channel.GetMessageAsync(starData.StarboardMessageId);
        }
        catch (Exception)
        {
            _logger.Error("{Source}: Message with id {Id} not found", nameof(StarboardService),
                starData.DiscordMessageId);
            return;
        }
        
        await message.ModifyAsync(await BuildStarboardMessage(starData, emoji));
    }
    
    private async Task CreateStarboardMessage
    (
        StarboardMessageDbEntity starData,
        GuildConfigDbEntity config,
        DiscordEmoji emoji
    )
    {
        DiscordMessageBuilder starboardMessageBuilder = await BuildStarboardMessage(starData, emoji);
        
        DiscordChannel starboardChannel = await _client.GetChannelAsync(config.StarboardChannelId!.Value);
        
        DiscordMessage starboardMessage = await starboardChannel.SendMessageAsync(starboardMessageBuilder);
        
        await using MadsContext db = await _dbFactory.CreateDbContextAsync();
        
        StarboardMessageDbEntity? starDataOld = db.Starboard.FirstOrDefault(
            x => x.DiscordMessageId == starData.DiscordMessageId && x.DiscordChannelId == starData.DiscordChannelId
        );
        
        if (starDataOld is null)
        {
            return;
        }
        
        starDataOld.StarboardMessageId = starboardMessage.Id;
        starDataOld.StarboardChannelId = starboardMessage.ChannelId;
        starDataOld.StarboardGuildId = starboardMessage.Channel!.GuildId!.Value;
        
        db.Update(starDataOld);
        await db.SaveChangesAsync();
    }
    
    private async Task<DiscordMessageBuilder> BuildStarboardMessage
    (
        StarboardMessageDbEntity starData,
        DiscordEmoji emoji
    )
    {
        DiscordMessage message;
        DiscordChannel channel;
        try
        {
            channel = await _client.GetChannelAsync(starData.DiscordChannelId);
            message = await channel.GetMessageAsync(starData.DiscordMessageId);
        }
        catch (Exception)
        {
            _logger.Error("{Source}: Message with id {Id} not found", nameof(StarboardService),
                starData.DiscordMessageId);
            throw;
        }
        
        
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithAuthor($"{message.Author!.Username}#{message.Author.Discriminator}",
                iconUrl: string.IsNullOrEmpty(message.Author.AvatarHash)
                    ? message.Author.DefaultAvatarUrl
                    : message.Author.AvatarUrl)
            .WithDescription(message.Content.Truncate(800, "..."))
            .WithFooter($"ID: {message.Id}")
            .WithTimestamp(message.Id);
        
        List<DiscordAttachment> imageAttachments = message.Attachments.Where(
                x => x.Url!.ToLower().EndsWith(".jpg") ||
                     x.Url.ToLower().EndsWith(".png") ||
                     x.Url.ToLower().EndsWith(".jpeg") ||
                     x.Url.ToLower().EndsWith(".gif"))
            .ToList();
        
        if (imageAttachments.Any())
        {
            embed.WithImageUrl(imageAttachments.First().Url!);
        }
        
        
        string emotename = emoji.GetDiscordName().Replace(":", "");
        emotename = emotename.EndsWith('s') ? emotename : starData.Stars > 1 ? emotename + "s" : emotename;
        
        if (message.ReferencedMessage is not null)
        {
            string refContent = message.ReferencedMessage.Content.Truncate(200, "...").Replace(")[", "​)[") + " ";
            
            embed.Description +=
                $"\n\n**➥** {message.ReferencedMessage.Author!.Mention}: {refContent} {(message.ReferencedMessage.Attachments.Any() ? $"_<{message.ReferencedMessage.Attachments.Count} file(s)>_" : "")}";
        }
        
        DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
            .AddEmbed(embed)
            .WithContent($"{emoji} {starData.Stars} {emotename} in {message.Channel!.Mention}");
        
        messageBuilder.AddComponents(new DiscordLinkButtonComponent(message.JumpLink.ToString(), "Go to message"));
        
        return messageBuilder;
    }
}