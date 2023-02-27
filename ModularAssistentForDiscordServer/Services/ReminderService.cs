using DSharpPlus;
using MADS.Entities;
using MADS.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace MADS.Services;

public class ReminderService : IHostedService
{
    private readonly PeriodicTimer _timer;
    private List<ReminderDbEntity> _reminders;
    private bool _isDisposed;

    private bool _isRunning;
    private Task _workerThread;
    private DiscordClient _client;
    private readonly IDbContextFactory<MadsContext> _dbContextFactory;


    public ReminderService(IDbContextFactory<MadsContext> dbContextFactory, DiscordClient client)
    {
        _dbContextFactory = dbContextFactory;
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        _client = client;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _isRunning = true;
        _workerThread = Worker();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _isDisposed = true;
        _isRunning = false;
    }

    private async Task Worker()
    {
        while (await _timer.WaitForNextTickAsync() && !_isDisposed)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            _reminders = db.Reminders.ToList();

            var recentReminder = _reminders.Where(x => (x.ExecutionTime - DateTime.UtcNow) <= TimeSpan.FromMinutes(5))
                .ToList();

            if (!recentReminder.Any()) continue;

            foreach (var reminder in recentReminder)
            {
                Task.Run(async () => await DispatchReminder(reminder, reminder.ExecutionTime - DateTime.UtcNow));
            }

            db.Reminders.RemoveRange(recentReminder);
            await db.SaveChangesAsync();
        }
    }

    private async Task DispatchReminder(ReminderDbEntity reminder, TimeSpan delay)
    {
        await Task.Delay(delay);

        var channel = await _client.GetChannelAsync(reminder.ChannelId);
        await channel.SendMessageAsync(reminder.GetMessage());
    }

    public async void AddReminder(ReminderDbEntity reminder)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        db.Reminders.Add(reminder);
        await db.SaveChangesAsync();
    }
}