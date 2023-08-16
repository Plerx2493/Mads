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
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using MADS.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Serilog;
// ReSharper disable HeuristicUnreachableCode

namespace MADS.Services;

public class VoiceAlertService : IHostedService
{
    private readonly IDbContextFactory<MadsContext> _contextFactory;
    private readonly DiscordClient _discordClient;
    
    public VoiceAlertService(IDbContextFactory<MadsContext> contextFactory, DiscordClientService discordClientService)
    {
        _discordClient = discordClientService.DiscordClient;
        _contextFactory = contextFactory;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _discordClient.VoiceStateUpdated += HandleEvent;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _discordClient.VoiceStateUpdated -= HandleEvent;
        return Task.CompletedTask;
    }
    
    public async Task HandleEvent(DiscordClient client, VoiceStateUpdateEventArgs e)
    {
        if (e.After.Channel is null) return;
        
        await using var context = _contextFactory.CreateDbContext();
        var alerts = await context.VoiceAlerts
            .Where(x => x.ChannelId == e.After.Channel.Id)
            .ToListAsync();
        
        if (!alerts.Any()) return;
        
        var embed = new DiscordEmbedBuilder
        {
            Title = "Voice Alert",
            Description = $"{e.User.Mention} joined {e.After.Channel.Mention}",
            Color = DiscordColor.Green
        };
        
        foreach (var alert in alerts)
        {
            //if (e.User.Id == alert.UserId) continue;
            try
            {
                var member = await e.Guild.GetMemberAsync(alert.UserId);
                if (member is null) continue;

                await member.SendMessageAsync(embed);
                
                if (!alert.IsRepeatable) context.VoiceAlerts.Remove(alert); 
            }
            catch (DiscordException exception)
            {
                Log.Error(exception, "Failed to send voice alert to {UserId}", alert.UserId);
            }
        }
        
        await context.SaveChangesAsync();
    }
    
    public async Task AddVoiceAlertAsync(ulong userId, ulong channelId, ulong guildId, bool isRepeatable = false)
    {
        await using var context = _contextFactory.CreateDbContext();
        var user = await context.Users
            .Include(x => x.VoiceAlerts)
            .FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            user = new UserDbEntity
            {
                Id = userId,
                VoiceAlerts = new List<VoiceAlert>()
            };
            await context.Users.AddAsync(user);
        }

        var alert = new VoiceAlert
        {
            ChannelId = channelId,
            GuildId = guildId,
            UserId = userId,
            IsRepeatable = isRepeatable
        };
        await context.VoiceAlerts.AddAsync(alert);
        await context.SaveChangesAsync();
    }
    
    public async Task RemoveVoiceAlert(ulong userId, ulong channelId, ulong guildId)
    {
        await using var context = _contextFactory.CreateDbContext();
        var user = await context.Users
            .Include(x => x.VoiceAlerts)
            .FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            return;
        }

        var alert = user.VoiceAlerts.FirstOrDefault(x => x.ChannelId == channelId && x.GuildId == guildId);
        if (alert == null)
        {
            return;
        }

        context.VoiceAlerts.Remove(alert);
        await context.SaveChangesAsync();
    }
    
    public async Task RemoveVoiceAlert(ulong alertId)
    {
        await using var context = _contextFactory.CreateDbContext();
        var alert = await context.VoiceAlerts.FirstOrDefaultAsync(x => x.AlertId == alertId);
        if (alert == null)
        {
            return;
        }

        context.VoiceAlerts.Remove(alert);
        await context.SaveChangesAsync();
    }
    
    public async Task<IEnumerable<VoiceAlert>> GetVoiceAlerts(ulong userId)
    {
        await using var context = _contextFactory.CreateDbContext();
        var user = await context.Users
            .Include(x => x.VoiceAlerts)
            .FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            return Enumerable.Empty<VoiceAlert>();
        }

        return user.VoiceAlerts;
    }
}