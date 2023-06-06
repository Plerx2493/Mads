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
using MADS.Services;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Serilog;

namespace MADS.Services;

public class ReminderService : IHostedService
{
    private bool _isDisposed;
    private bool _isRunning;

    private IScheduler _reminderScheduler =>
        _schedulerFactory.GetScheduler("reminder-scheduler").GetAwaiter().GetResult();

    private DiscordClient _client;
    private DiscordRestClient _restClient;
    private readonly IDbContextFactory<MadsContext> _dbContextFactory;
    private readonly ISchedulerFactory _schedulerFactory;

    public ReminderService(IDbContextFactory<MadsContext> dbContextFactory, ISchedulerFactory schedulerFactory,
        DiscordClient client, DiscordRestClient rest)
    {
        _dbContextFactory = dbContextFactory;
        _schedulerFactory = schedulerFactory;
        _client = client;
        _restClient = rest;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_isRunning) return;
        if (_isDisposed) throw new UnreachableException();
        //_reminderScheduler = await _schedulerFactory.GetScheduler("reminder-scheduler");
        if (_reminderScheduler == null) throw new NullReferenceException();
        _reminderScheduler.Start();
        _isRunning = true;
        _client.Logger.LogInformation("Reminders active");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (!_isRunning) return;
        if (_isDisposed) return;
        _isDisposed = true;
        _isRunning = false;
        _reminderScheduler.Shutdown();
        _client.Logger.LogInformation("Reminders stopped");
    }


    private async Task DispatchReminder(ReminderDbEntity reminder)
    {
        DiscordChannel channel;
        if (!reminder.IsPrivate)
        {
            channel = await _client.GetChannelAsync(reminder.ChannelId);
        }
        else
        {
            channel = await _restClient.CreateDmAsync(reminder.UserId);
        }

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
            db.Reminders.First(x => x.UserId == reminder.UserId && x.ExecutionTime == reminder.ExecutionTime);

        var jobKey = new JobKey($"reminder-{dbEntity.Id}", "reminders");
        var triggerKey = new TriggerKey($"reminder-trigger-{dbEntity.Id}", "reminders");

        var job = JobBuilder.Create<ReminderJob>()
            .UsingJobData("reminderId", dbEntity.Id)
            .WithIdentity(jobKey)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity(triggerKey)
            .StartAt((DateTimeOffset) reminder.ExecutionTime)
            .Build();

        await _reminderScheduler.ScheduleJob(job, trigger);
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

        try
        {
            reminder = db.Reminders.First(x => x.Id == reminderId);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        if (reminder is null) return false;

        var jobKey = new JobKey($"reminder-{reminder.Id}", "reminders");
        await _reminderScheduler.DeleteJob(jobKey);

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

    private class ReminderJob : IJob
    {
        private DiscordClient _client;
        private ReminderService _reminder;
        private readonly IDbContextFactory<MadsContext> _dbContextFactory;

        public ReminderJob(IDbContextFactory<MadsContext> dbContextFactory, DiscordClient client,
            ReminderService reminderService)
        {
            _reminder = reminderService;
            _dbContextFactory = dbContextFactory;
            _client = client;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var key = context.JobDetail.Key;

            // note: use context.MergedJobDataMap in production code
            var dataMap = context.MergedJobDataMap;

            var reminderId = Convert.ToUInt64(dataMap.Get("reminderId"));

            var reminder = await _reminder.TryGetByIdAsync(reminderId);

            if (reminder is null)
            {
                Log.Warning("Tried to dispatch a nonexistent reminder: {Id} ",
                    Convert.ToUInt64(dataMap.Get("reminderId")));
                return;
            }

            Log.Warning("Dispatching reminder id: {Id}", reminder.Id);

            await _reminder.DispatchReminder(reminder);
        }
    }
}