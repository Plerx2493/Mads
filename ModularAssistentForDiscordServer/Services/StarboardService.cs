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

#nullable enable
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Humanizer;
using MADS.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MADS.Services;

public class StarboardService : IHostedService
{
    private readonly DiscordClient _client;
    private readonly IDbContextFactory<MadsContext> _dbFactory;
    private readonly Queue<DiscordReactionUpdateEvent> _messageQueue;
    private bool _active;

    public StarboardService(DiscordClientService client, IDbContextFactory<MadsContext> dbFactory)
    {
        _client = client.DiscordClient;
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
        _client.Logger.LogInformation("Starboard active");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _client.MessageReactionAdded -= HandleReactionAdded;
        _client.MessageReactionRemoved -= HandleReactionRemoved;
        _client.MessageReactionsCleared -= HandleReactionsCleared;
        _client.MessageReactionRemovedEmoji -= HandleReactionEmojiRemoved;
        _client.Logger.LogInformation("Starboard stopped");
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
        var _ = Task.Run(HandleQueue);
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
            try
            {
                var msg = _messageQueue.Dequeue();
                await HandleEvent(msg);
            }
            catch (Exception e)
            {
                await MainProgram.LogToWebhookAsync(e);
            }


        _active = false;
    }

    private async Task HandleEvent(DiscordReactionUpdateEvent e)
    {
        if (!e.Message.Channel.GuildId.HasValue) return;

        if (e.Type == DiscordReactionUpdateType.ReactionAdded)
        {
            var eventArgs = (MessageReactionAddEventArgs) e.EventArgs;
            if (eventArgs.User.IsBot) return;
        }


        var db = await _dbFactory.CreateDbContextAsync();

        var guildSettings = db.Configs.First(x => x.DiscordGuildId == e.Message.Channel.GuildId);

        if (!guildSettings.StarboardActive) return;

        if (guildSettings.StarboardEmojiId == null
            || guildSettings.StarboardThreshold == null
            || guildSettings.StarboardChannelId == null)
        {
            _client.Logger.LogError("GuildSettings incomplete: StarboardSettings");
            return;
        }

        DiscordEmoji discordEmoji;

        switch (e.Type)
        {
            case DiscordReactionUpdateType.ReactionAdded:
            {
                var eventArgs = (MessageReactionAddEventArgs) e.EventArgs;

                if (guildSettings.StarboardEmojiId != 0)
                    discordEmoji = DiscordEmoji.FromGuildEmote(_client, guildSettings.StarboardEmojiId.Value);
                else
                    discordEmoji = DiscordEmoji.FromUnicode(_client, guildSettings.StarboardEmojiName);

                if (eventArgs.Emoji != discordEmoji) return;
                break;
            }

            case DiscordReactionUpdateType.ReactionRemoved:
            {
                var eventArgs = (MessageReactionRemoveEventArgs) e.EventArgs;

                if (guildSettings.StarboardEmojiId != 0)
                    discordEmoji = DiscordEmoji.FromGuildEmote(_client, guildSettings.StarboardEmojiId.Value);
                else
                    discordEmoji = DiscordEmoji.FromUnicode(_client, guildSettings.StarboardEmojiName);

                if (eventArgs.Emoji != discordEmoji) return;
                break;
            }

            case DiscordReactionUpdateType.ReactionsCleard:
                var msgs = db.Starboard.Where(x =>
                    x.DiscordMessageId == e.Message.Id && x.DiscordChannelId == e.Message.ChannelId);
                db.Starboard.RemoveRange(msgs);
                await db.SaveChangesAsync();
                return;

            case DiscordReactionUpdateType.ReactionEmojiRemoved:
                var argss = (MessageReactionRemoveEmojiEventArgs) e.EventArgs;

                if (guildSettings.StarboardEmojiId != 0)
                    discordEmoji = DiscordEmoji.FromGuildEmote(_client, guildSettings.StarboardEmojiId.Value);
                else
                    discordEmoji = DiscordEmoji.FromUnicode(_client, guildSettings.StarboardEmojiName);

                if (argss.Emoji != discordEmoji) return;
                var msgss = db.Starboard.Where(x =>
                    x.DiscordMessageId == e.Message.Id && x.DiscordChannelId == e.Message.ChannelId);
                db.Starboard.RemoveRange(msgss);
                return;

            default:
                throw new ArgumentOutOfRangeException();
        }


        StarboardMessageDbEntity? starData = null;
        bool isNew;
        try
        {
            starData = db.Starboard.First(
                x => x.DiscordMessageId == e.Message.Id && x.DiscordChannelId == e.Message.ChannelId
            );
            if (e.Type is DiscordReactionUpdateType.ReactionAdded)
                starData.Stars++;
            else
                starData.Stars--;

            isNew = false;
        }
        catch (Exception)
        {
            isNew = true;
        }

        if (isNew)
            starData = new StarboardMessageDbEntity
            {
                DiscordChannelId = e.Message.ChannelId,
                DiscordMessageId = e.Message.Id,
                DiscordGuildId = e.Message.Channel.Guild.Id,
                Stars = 1
            };

        if (starData is null) throw new ArgumentNullException();

        if (isNew)
            db.Starboard.Add(starData);
        else
            db.Starboard.Update(starData);

        await db.SaveChangesAsync();

        if (starData.Stars < guildSettings.StarboardThreshold)
        {
            if (isNew) return;
            if (starData.StarboardMessageId != 0)
                await DeleteStarboardMessage(starData);


            if (starData.Stars == 0 && !isNew)
                db.Starboard.Remove(starData);

            await db.SaveChangesAsync();
            await db.DisposeAsync();
            return;
        }

        if (starData.StarboardMessageId == 0)
        {
            await CreateStarboardMessage(starData, guildSettings, discordEmoji);
            await db.DisposeAsync();
            return;
        }

        await UpdateStarboardMessage(starData, discordEmoji);
        await db.DisposeAsync();
    }

    private async Task DeleteStarboardMessage(StarboardMessageDbEntity starData)
    {
        DiscordMessage message;

        try
        {
            message = await _client.GetChannelAsync(starData.StarboardChannelId).Result
                .GetMessageAsync(starData.StarboardMessageId);
        }
        catch (Exception)
        {
            Console.WriteLine("StarboardMessage not found");
            throw;
        }

        await message.DeleteAsync();

        var db = await _dbFactory.CreateDbContextAsync();

        var starDataOld = db.Starboard.First(
            x => x.DiscordMessageId == starData.DiscordMessageId && x.DiscordChannelId == starData.DiscordChannelId
        );

        starDataOld.StarboardMessageId = 0;
        starDataOld.StarboardChannelId = 0;
        starDataOld.StarboardGuildId = 0;

        db.Update(starDataOld);
        await db.SaveChangesAsync();
        await db.DisposeAsync();
    }

    private async Task UpdateStarboardMessage(StarboardMessageDbEntity starData, DiscordEmoji emoji)
    {
        DiscordMessage message;

        try
        {
            message = await _client.GetChannelAsync(starData.StarboardChannelId).Result
                .GetMessageAsync(starData.StarboardMessageId);
        }
        catch (Exception)
        {
            Console.WriteLine("StarboardMessage not found");
            throw;
        }

        await message.ModifyAsync(await BuildStarboardMessage(starData, emoji));
    }

    private async Task CreateStarboardMessage
    (
        StarboardMessageDbEntity starData,
        GuildConfigDbEntity congfig,
        DiscordEmoji emoji
    )
    {
        var starboardMessageBuilder = await BuildStarboardMessage(starData, emoji);

        var starboardChannel = await _client.GetChannelAsync(congfig.StarboardChannelId!.Value);

        var starboardMessage = await starboardChannel.SendMessageAsync(starboardMessageBuilder);

        var db = await _dbFactory.CreateDbContextAsync();

        var starDataOld = db.Starboard.First(
            x => x.DiscordMessageId == starData.DiscordMessageId && x.DiscordChannelId == starData.DiscordChannelId
        );

        starDataOld.StarboardMessageId = starboardMessage.Id;
        starDataOld.StarboardChannelId = starboardMessage.ChannelId;
        starDataOld.StarboardGuildId = starboardMessage.Channel.GuildId!.Value;

        db.Update(starDataOld);
        await db.SaveChangesAsync();
        await db.DisposeAsync();
    }

    private async Task<DiscordMessageBuilder> BuildStarboardMessage
    (
        StarboardMessageDbEntity starData,
        DiscordEmoji emoji
    )
    {
        DiscordMessage message;

        try
        {
            message = await _client.GetChannelAsync(starData.DiscordChannelId).Result
                .GetMessageAsync(starData.DiscordMessageId);
        }
        catch (Exception)
        {
            Console.WriteLine("starred message not found");
            throw;
        }


        var embed = new DiscordEmbedBuilder()
            .WithAuthor($"{message.Author.Username}#{message.Author.Discriminator}",
                iconUrl: string.IsNullOrEmpty(message.Author.AvatarHash)
                    ? message.Author.DefaultAvatarUrl
                    : message.Author.AvatarUrl)
            .WithDescription(message.Content.Truncate(800, "..."))
            .WithFooter($"ID: {message.Id}")
            .WithTimestamp(message.Id);

        var imageAttachments = message.Attachments.Where(
                x => x.Url.ToLower().EndsWith(".jpg") ||
                     x.Url.ToLower().EndsWith(".png") ||
                     x.Url.ToLower().EndsWith(".jpeg") ||
                     x.Url.ToLower().EndsWith(".gif"))
            .ToList();

        if (imageAttachments.Any()) embed.WithImageUrl(imageAttachments.First().Url);


        var emotename = emoji.GetDiscordName().Replace(":", "");
        emotename = emotename.EndsWith('s') ? emotename : starData.Stars > 1 ? emotename + "s" : emotename;

        if (message.ReferencedMessage != null)
        {
            var refContent = message.ReferencedMessage.Content.Truncate(200, "...").Replace(")[", "​)[") + " ";

            embed.Description +=
                $"\n\n**➥** {message.ReferencedMessage.Author.Mention}: {refContent} {(message.ReferencedMessage.Attachments.Any() ? $"_<{message.ReferencedMessage.Attachments.Count} file(s)>_" : "")}";
        }

        var messageBuilder = new DiscordMessageBuilder()
            .AddEmbed(embed)
            .WithContent($"{emoji} {starData.Stars} {emotename} in {message.Channel.Mention}");

        messageBuilder.AddComponents(new DiscordLinkButtonComponent(message.JumpLink.ToString(), "Go to message"));

        return messageBuilder;
    }
}