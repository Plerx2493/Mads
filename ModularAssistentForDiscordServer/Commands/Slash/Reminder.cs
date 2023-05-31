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

using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Humanizer;
using MADS.Commands.AutoCompletion;
using MADS.Entities;
using MADS.Services;

namespace MADS.Commands.Slash;

[SlashCommandGroup("reminder", "mangage reminders")]
public class Reminder : MadsBaseApplicationCommand
{
    public ReminderService ReminderService;

    [SlashCommand("add", "add new reminder")]
    public async Task AddReminder
    (
        InteractionContext ctx,
        [Option("timespan", "when the reminder should trigger")]
        TimeSpan? timeSpan,
        [Option("text", "text")] string text
    )
    {
        await ctx.DeferAsync(true);

        if (timeSpan is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Invalid timespan (5s, 3m, 7h, 2d)"));
            return;
        }

        if (timeSpan < TimeSpan.FromSeconds(30))
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("Can not create reminder with a timespan under 30 seconds"));
            return;
        }

        var newReminder = new ReminderDbEntity
        {
            UserId = ctx.User.Id,
            ChannelId = ctx.Channel.Id,
            CreationTime = DateTime.UtcNow,
            ExecutionTime = DateTime.UtcNow + timeSpan.Value,
            ReminderText = text,
            IsPrivate = false
        };

        ReminderService.AddReminder(newReminder);

        await ctx.EditResponseAsync(
            new DiscordWebhookBuilder().WithContent(
                $"Reminder created. I will remind you in {Formatter.Timestamp(timeSpan.Value)}"));
    }

    [SlashCommand("list", "list your Reminder")]
    public async Task ListReminder
    (
        InteractionContext ctx
    )
    {
        await ctx.DeferAsync(true);

        var reminders = await ReminderService.GetByUserAsync(ctx.User.Id);
        var remindersTextList = reminders
            .Select(x =>
                $"```-Id: {x.Id}\n-Remindertext:\n{x.ReminderText}\n-ExecutionTime: {x.ExecutionTime.Humanize()}```");

        var reminderText = new StringBuilder().AppendJoin("\n", remindersTextList);

        var embed = new DiscordEmbedBuilder()
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
        var reminder = await ReminderService.TryGetByIdAsync((ulong) id);

        if (reminder is null)
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Reminder does not exsists"));
            return;
        }

        var success = await ReminderService.TryDeleteById((ulong) id);

        if (!success)
        {
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().WithContent("Something went wrong. Please try again"));
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle("Reminder removed")
            .WithDescription(
                $"```-Id: {reminder.Id}\n-Remindertext:\n{reminder.ReminderText}```\nWould have fired {reminder.GetTimestamp()}");

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }
}