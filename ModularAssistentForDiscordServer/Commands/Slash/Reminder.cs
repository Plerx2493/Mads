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

using System.ComponentModel;
using System.Text;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using Humanizer;
using MADS.Commands.AutoCompletion;
using MADS.Entities;
using MADS.Extensions;
using MADS.Services;

namespace MADS.Commands.Slash;

[Command("reminder"), Description("mangage reminders")]
public sealed class Reminder
{
    private readonly ReminderService _reminderService;
    private readonly UserManagerService _userManagerService;
    
    public Reminder(ReminderService reminderService, UserManagerService userManagerService)
    {
        _reminderService = reminderService;
        _userManagerService = userManagerService;
    }

    [Command("add"), Description("add new reminder")]
    public async Task AddReminder
    (
        CommandContext ctx,
        [Description("when the reminder should trigger")]
        TimeSpan? timeSpan,
        [Description("text")] string text,
        [Description("Sets if the reminder should be executed in your DMs")]
        bool isPrivate = false
    )
    {
        await ctx.DeferAsync(true);

        if (timeSpan is null)
        {
            await ctx.EditResponse_Error("Invalid timespan (5s, 3m, 7h, 2d)");
            return;
        }
        
        await _userManagerService.GetOrCreateUserAsync(ctx.User);

        ReminderDbEntity newReminder = new()
        {
            UserId = ctx.User.Id,
            ChannelId = ctx.Channel.Id,
            CreationTime = DateTime.UtcNow,
            ExecutionTime = DateTime.UtcNow + timeSpan.Value,
            ReminderText = text,
            IsPrivate = isPrivate
        };

        ReminderDbEntity reminder = await _reminderService.AddReminder(newReminder);

        await ctx.EditResponse_Success($"Reminder created with id `{reminder.Id}`. I will remind you in {Formatter.Timestamp(timeSpan.Value)}");
    }

    [Command("list"), Description("list your Reminder")]
    public async Task ListReminder
    (
        CommandContext ctx
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

    [Command("delete"), Description("delete a reminder based on its id")]
    public async Task DeleteById
    (
        CommandContext ctx,
        [SlashAutoCompleteProvider(typeof(ReminderAutoCompletion)),
         Description("id of the given reminder which should be deleted")]
        long id
    )
    {
        await ctx.DeferAsync(true);
        ReminderDbEntity? reminder = await _reminderService.TryGetByIdAsync((ulong) id);

        if (reminder is null)
        {
            await ctx.EditResponse_Error("Reminder does not exists");
            return;
        }

        bool success = await _reminderService.TryDeleteById((ulong) id, ctx.User.Id);

        if (!success)
        {
            await ctx.EditResponse_Error("Something went wrong. Are you sure you own this reminder?");
            return;
        }

        DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithTitle("Reminder removed")
            .WithDescription(
                $"```-Id: {reminder.Id}\n-Remindertext:\n{reminder.ReminderText}```\nWould have fired {reminder.GetExecutionTimestamp()}");

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }
}