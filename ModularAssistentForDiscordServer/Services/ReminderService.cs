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
using MADS.Entities;
using MADS.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MADS.Services;

public class ReminderService : IHostedService
{
    private readonly PeriodicTimer _timer;
    private bool _isDisposed;
    private List<ulong> _activeReminder = new();

    private bool _isRunning;
    private Thread _workerThread;
    private DiscordClient _client;
    private readonly IDbContextFactory<MadsContext> _dbContextFactory;


    public ReminderService(IDbContextFactory<MadsContext> dbContextFactory, DiscordClient client)
    {
        _dbContextFactory = dbContextFactory;
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        _client = client;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_isRunning) return;

        _workerThread = new Thread(() => Worker())
        {
            // This is important as it allows the process to exit while this thread is running
            IsBackground = true
        };
        _workerThread.Start();

        _isRunning = true;
        _client.Logger.LogInformation("Reminders acitve");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _isDisposed = true;
        _isRunning = false;
        _workerThread.Interrupt();
        _client.Logger.LogInformation("Reminders stopped");
    }

    private async Task Worker()
    {
        while (!_isDisposed && await _timer.WaitForNextTickAsync())
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            var recentReminder = db.Reminders
                .Where(x => (x.ExecutionTime - DateTime.UtcNow).Milliseconds <= TimeSpan.FromMinutes(5).Milliseconds)
                .Where(x => !_activeReminder.Contains(x.Id))
                .ToList();

            if (!recentReminder.Any()) continue;

            foreach (var reminder in recentReminder)
            {
                _activeReminder.Add(reminder.Id);
                _ = Task.Run(async () => await DispatchReminder(reminder, reminder.ExecutionTime - DateTime.UtcNow));
            }
        }
    }

    private async Task DispatchReminder(ReminderDbEntity reminder, TimeSpan delay)
    {
        if (delay.Milliseconds > 0) await Task.Delay(delay);

        var channel = await _client.GetChannelAsync(reminder.ChannelId);
        await channel.SendMessageAsync(await reminder.GetMessageAsync(_client));
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        db.Reminders.Remove(reminder);
        _activeReminder.Remove(reminder.Id);
        await db.SaveChangesAsync();
    }

    public async void AddReminder(ReminderDbEntity reminder)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        db.Reminders.Add(reminder);
        await db.SaveChangesAsync();
    }

    public async Task<List<ReminderDbEntity>> GetByUserAsync(ulong userId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return db.Reminders.Where(x => x.UserId == userId).ToList();
    }

    public async Task<bool> TryDeleteById(ulong reminderId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        ReminderDbEntity reminder;

        try
        {
            reminder = db.Reminders.First(x => x.Id == reminderId);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        if (reminder is null) return false;

        db.Reminders.Remove(reminder);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<ReminderDbEntity?> TryGetByIdAsync(ulong id)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        ReminderDbEntity reminder;
        try
        {
            reminder = db.Reminders.First(x => x.Id == id);
        }
        catch (InvalidOperationException)
        {
            return null;
        }

        return reminder;
    }
}