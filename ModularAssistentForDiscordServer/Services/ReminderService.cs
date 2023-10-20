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

using System.Diagnostics;
using DSharpPlus;
using DSharpPlus.Entities;
using MADS.Entities;
using MADS.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Quartz;
using Serilog;
using ILogger = Serilog.ILogger;

namespace MADS.Services;

public class ReminderService : IHostedService
{
    private readonly IDbContextFactory<MadsContext> _dbContextFactory;
    private readonly ISchedulerFactory _schedulerFactory;

    private readonly DiscordClient _client;
    private bool _isDisposed;
    private bool _isRunning;
    private readonly DiscordRestClient _restClient;
    
    private static ILogger _logger = Log.ForContext<ReminderService>();

    public ReminderService(IDbContextFactory<MadsContext> dbContextFactory, ISchedulerFactory schedulerFactory,
        DiscordClient client, DiscordRestClient rest)
    {
        _dbContextFactory = dbContextFactory;
        _schedulerFactory = schedulerFactory;
        _client = client;
        _restClient = rest;
    }

    private IScheduler ReminderScheduler =>
        _schedulerFactory.GetScheduler("reminder-scheduler").GetAwaiter().GetResult();

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_isRunning) return;
        if (_isDisposed) throw new UnreachableException();
        //_reminderScheduler = await _schedulerFactory.GetScheduler("reminder-scheduler");
        if (ReminderScheduler == null) throw new NullReferenceException();
        await ReminderScheduler.Start();
        _isRunning = true;
        _logger.Information("Reminders active");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (!_isRunning) return;
        if (_isDisposed) return;
        _isDisposed = true;
        _isRunning = false;
        await ReminderScheduler.Shutdown();
        _logger.Information("Reminders stopped");
    }

    private async Task DispatchReminder(ReminderDbEntity reminder)
    {
        DiscordChannel channel;
        if (!reminder.IsPrivate)
            channel = await _client.GetChannelAsync(reminder.ChannelId);
        else
            channel = await _restClient.CreateDmAsync(reminder.UserId);

        await channel.SendMessageAsync(await reminder.GetMessageAsync(_client));
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        db.Reminders.Remove(reminder);
        await db.SaveChangesAsync();
    }

    public async Task<ReminderDbEntity> AddReminder(ReminderDbEntity reminder)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        db.Reminders.Add(reminder);

        await db.SaveChangesAsync();

        var dbEntity =
            db.Reminders.FirstOrDefault(x => x.UserId == reminder.UserId && x.ExecutionTime == reminder.ExecutionTime);

        ArgumentNullException.ThrowIfNull(dbEntity);
        
        var jobKey = new JobKey($"reminder-{dbEntity.Id}", "reminders");
        var triggerKey = new TriggerKey($"reminder-trigger-{dbEntity.Id}", "reminders");

        var job = JobBuilder.Create<ReminderJob>()
            .UsingJobData("reminderId", dbEntity.Id)
            .WithIdentity(jobKey)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity(triggerKey)
            .StartAt(reminder.ExecutionTime)
            .Build();

        await ReminderScheduler.ScheduleJob(job, trigger);
        return dbEntity;
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

        reminder = db.Reminders.FirstOrDefault(x => x.Id == reminderId);

        if (reminder is null) return false;

        var jobKey = new JobKey($"reminder-{reminder.Id}", "reminders");
        await ReminderScheduler.DeleteJob(jobKey);

        db.Reminders.Remove(reminder);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<ReminderDbEntity?> TryGetByIdAsync(ulong id)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        ReminderDbEntity reminder;
        reminder = db.Reminders.FirstOrDefault(x => x.Id == id);

        return reminder;
    }

    private class ReminderJob : IJob
    {
        private readonly ReminderService _reminder;

        public ReminderJob(ReminderService reminderService)
        {
            _reminder = reminderService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var dataMap = context.MergedJobDataMap;

            var reminderId = Convert.ToUInt64(dataMap.Get("reminderId"));

            var reminder = await _reminder.TryGetByIdAsync(reminderId);

            if (reminder is null)
            {
                _logger.Warning("Tried to dispatch a nonexistent reminder: {Id} ",
                    Convert.ToUInt64(dataMap.Get("reminderId")));
                return;
            }

            _logger.Information("Dispatching reminder id: {Id}", reminder.Id);

            await _reminder.DispatchReminder(reminder);
        }
    }
}