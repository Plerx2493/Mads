using System.Diagnostics;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MADS.Entities;
using MADS.Extensions;
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

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Reminder created. I will remind you in {Formatter.Timestamp(timeSpan.Value)}"));
    }
    
    [SlashCommand("list", "list your Reminder")]
    public async Task ListReminder
    (
        InteractionContext ctx
    )
    {
        await ctx.DeferAsync(true);
        
        var reminders = await ReminderService.GetByUserAsync(ctx.User.Id);
        var remindersTextList = reminders.Select(x => $"```-Id: {x.Id}\n-Remindertext:\n{x.ReminderText}```");
        
        var reminderText = new StringBuilder().AppendJoin("\n\n" ,remindersTextList);
        
        var embed = new DiscordEmbedBuilder()
            .WithTitle("Your reminders:")
            .WithDescription(reminderText.ToString());
        
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }

    [SlashCommand("delete", "delete a reminder based on its id")]
    public async Task DeleteById
    (
        InteractionContext ctx,
        [Option("id", "id of the given reminder which should be deleted")]
        long id
    )
    {
        await ctx.DeferAsync(true);
        var reminder = await ReminderService.TryGetByIdAsync((ulong)id);
        
        if (reminder is null )
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Reminder does not exsists"));
            return;
        }

        var success =  await ReminderService.TryDeleteById((ulong)id);
        
        if (!success) 
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Something went wrong. Please try again"));
            return;
        }
            
        var embed = new DiscordEmbedBuilder()
                    .WithTitle("Reminder removed")
                    .WithDescription($"```-Id: {reminder.Id}\n-Remindertext:\n{reminder.ReminderText}```\nWould have fired {reminder.GetTimestamp()}");
        
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }
}