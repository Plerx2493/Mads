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

using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Humanizer;
using MADS.Commands.AutoCompletion;
using MADS.Entities;
using MADS.Extensions;
using MADS.Services;

namespace MADS.Commands.Slash;

[SlashCommandGroup("reminder", "mangage reminders")]
public sealed class Reminder : MadsBaseApplicationCommand
{
    private ReminderService _reminderService;
    
    public Reminder(ReminderService reminderService)
    {
        _reminderService = reminderService;
    }

    [SlashCommand("add", "add new reminder")]
    public async Task AddReminder
    (
        InteractionContext ctx,
        [Option("timespan", "when the reminder should trigger")]
        TimeSpan? timeSpan,
        [Option("text", "text")] string text,
        [Option("private", "Sets if the reminder should be executed in your DMs")]
        bool isPrivate = false
    )
    {
        await ctx.DeferAsync(true);

        if (timeSpan is null)
        {
            await EditResponse_Error("Invalid timespan (5s, 3m, 7h, 2d)");
            return;
        }

        ReminderDbEntity newReminder = new()
        {
            UserId = ctx.User.Id,
            ChannelId = ctx.Channel.Id,
            CreationTime = DateTime.UtcNow,
            ExecutionTime = DateTime.UtcNow + timeSpan.Value,
            ReminderText = text,
            IsPrivate = isPrivate
        };

        await _reminderService.AddReminder(newReminder);

        await EditResponse_Success($"Reminder created. I will remind you in {Formatter.Timestamp(timeSpan.Value)}");
    }

    [SlashCommand("list", "list your Reminder")]
    public async Task ListReminder
    (
        InteractionContext ctx
    )
    {
        await ctx.DeferAsync(true);

        List<ReminderDbEntity> reminders = await _reminderService.GetByUserAsync(ctx.User.Id);
        IEnumerable<string> remindersTextList = reminders
            .Select(x =>
                $"```-Id: {x.Id}\n-Remindertext:\n {x.ReminderText}\n-ExecutionTime: {x.ExecutionTime.Humanize()}\n ({x.ExecutionTime.ToUniversalTime()} UTC)```");

        StringBuilder reminderText = new StringBuilder().AppendJoin("\n", remindersTextList);

        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithTitle("Your reminders:")
            .WithDescription(reminderText.ToString());

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }

    [SlashCommand("delete", "delete a reminder based on its id")]
    public async Task DeleteById
    (
        InteractionContext ctx,
        [Autocomplete(typeof(ReminderAutoCompletion)),
         Option("id", "id of the given reminder which should be deleted", true)]
        long id
    )
    {
        await ctx.DeferAsync(true);
        ReminderDbEntity? reminder = await _reminderService.TryGetByIdAsync((ulong) id);

        if (reminder is null)
        {
            await EditResponse_Error("Reminder does not exists");
            return;
        }

        bool success = await _reminderService.TryDeleteById((ulong) id);

        if (!success)
        {
            await EditResponse_Error("Something went wrong. Please try again");
            return;
        }

        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithTitle("Reminder removed")
            .WithDescription(
                $"```-Id: {reminder.Id}\n-Remindertext:\n{reminder.ReminderText}```\nWould have fired {reminder.GetExecutionTimestamp()}");

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }
}