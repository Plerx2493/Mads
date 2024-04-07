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
using DSharpPlus.Exceptions;
using MADS.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace MADS.Services;

public class VoiceAlertService : IHostedService
{
    private readonly IDbContextFactory<MadsContext> _contextFactory;
    private readonly DiscordClient _discordClient;
    private readonly Channel<VoiceStateUpdateEventArgs> _eventChannel;
    
    private static readonly ILogger _logger = Log.ForContext<VoiceAlertService>();
    private bool _stopped;
    private CancellationTokenSource _cts = new();
    private Task? _handleQueueTask;
    
    public VoiceAlertService(IDbContextFactory<MadsContext> contextFactory, DiscordClientService discordClientService)
    {
        _discordClient = discordClientService.DiscordClient;
        _contextFactory = contextFactory;
        _eventChannel = Channel.CreateUnbounded<VoiceStateUpdateEventArgs>();
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _discordClient.VoiceStateUpdated += AddEvent;
        _stopped = false;
        _handleQueueTask = Task.Factory.StartNew(HandleQueueAsync, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        _cts = new CancellationTokenSource();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _discordClient.VoiceStateUpdated -= AddEvent;
        _stopped = true;
        _cts.Cancel();
        _handleQueueTask?.Dispose();
        _handleQueueTask = null;
        _cts.Dispose();
        return Task.CompletedTask;
    }
    
    private async Task AddEvent(DiscordClient client, VoiceStateUpdateEventArgs e)
    {
        await _eventChannel.Writer.WriteAsync(e);
    }
    
    private async Task HandleQueueAsync()
    {
        while (!_stopped)
        {
            try
            {
                VoiceStateUpdateEventArgs e = await _eventChannel.Reader.ReadAsync(_cts.Token);
                await HandleEvent(e);
            }
            catch (Exception e)
            {
                _logger.Error("Exception in {Source}: {Message}", nameof(VoiceAlertService), e.Message);
            }
        }
    }

    private async Task HandleEvent(VoiceStateUpdateEventArgs e)
    {
        if (e.After.Channel is null)
        {
            return;
        }

        if (e.Before?.Channel?.Id == e.After.Channel.Id)
        {
            return;
        }

        await using MadsContext context = await _contextFactory.CreateDbContextAsync();
        List<VoiceAlert> alerts = await context.VoiceAlerts
            .AsNoTracking()
            .Where(x => x.ChannelId == e.After.Channel.Id && e.User.Id != x.UserId)
            .ToListAsync();
        
        if (!alerts.Any())
        {
            return;
        }

        DiscordEmbedBuilder embed = new()
        {
            Title = "Voice Alert",
            Description = $"{e.User.Mention} joined {e.After.Channel.Mention}",
            Color = DiscordColor.Green
        };
        
        foreach (VoiceAlert alert in alerts)
        {
            if (e.Channel.Users.Any(x => x.Id == alert.UserId))
            {
                continue;
            }

            if (alert.MinTimeBetweenAlerts is not null)
            {
                if (alert.LastAlert is not null && alert.LastAlert + alert.MinTimeBetweenAlerts > DateTimeOffset.UtcNow)
                {
                    continue;
                }
            }
            
            try
            {
                DiscordMember member = await e.Guild.GetMemberAsync(alert.UserId);

                await member.SendMessageAsync(embed);
                
                alert.LastAlert = DateTimeOffset.UtcNow;
                
                if (!alert.IsRepeatable)
                {
                    context.VoiceAlerts.Remove(alert);
                }
            }
            catch (DiscordException exception)
            {
                _logger.Error(exception, "Failed to send voice alert to {UserId}", alert.UserId);
            }
        }
        
        await context.SaveChangesAsync();
    }
    
    public async Task AddVoiceAlertAsync(ulong userId, ulong channelId, ulong guildId, bool isRepeatable = false, TimeSpan minTimeBetweenAlerts = new())
    {
        await using MadsContext context = await _contextFactory.CreateDbContextAsync();
        UserDbEntity? user = await context.Users
            .AsNoTracking()
            .Include(x => x.VoiceAlerts)
            .FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            user = new UserDbEntity
            {
                Id = userId,
                VoiceAlerts = [],
                PreferedLanguage = "en-US"
            };
            await context.Users.AddAsync(user);
        }

        VoiceAlert alert = new()
        {
            ChannelId = channelId,
            GuildId = guildId,
            UserId = userId,
            IsRepeatable = isRepeatable
        };
        
        if (minTimeBetweenAlerts != TimeSpan.Zero)
        {
            alert.MinTimeBetweenAlerts = minTimeBetweenAlerts;
        }
        
        _logger.Information("Voicealert added");
        
        await context.VoiceAlerts.AddAsync(alert);
        await context.SaveChangesAsync();
    }
    
    public async Task RemoveVoiceAlert(ulong userId, ulong channelId, ulong guildId)
    {
        await using MadsContext context = await _contextFactory.CreateDbContextAsync();
        UserDbEntity? user = await context.Users
            .AsNoTracking()
            .Include(x => x.VoiceAlerts)
            .FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            return;
        }

        VoiceAlert? alert = user.VoiceAlerts.FirstOrDefault(x => x.ChannelId == channelId && x.GuildId == guildId);
        if (alert == null)
        {
            return;
        }

        context.VoiceAlerts.Remove(alert);
        await context.SaveChangesAsync();
    }
    
    public async Task RemoveVoiceAlert(ulong alertId)
    {
        await using MadsContext context = await _contextFactory.CreateDbContextAsync();
        VoiceAlert? alert = await context.VoiceAlerts.FirstOrDefaultAsync(x => x.AlertId == alertId);
        if (alert == null)
        {
            return;
        }

        context.VoiceAlerts.Remove(alert);
        await context.SaveChangesAsync();
    }
    
    public async Task<IEnumerable<VoiceAlert>> GetVoiceAlerts(ulong userId)
    {
        await using MadsContext context = await _contextFactory.CreateDbContextAsync();
        UserDbEntity? user = await context.Users
            .Include(x => x.VoiceAlerts)
            .FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            return Enumerable.Empty<VoiceAlert>();
        }

        return user.VoiceAlerts;
    }
}