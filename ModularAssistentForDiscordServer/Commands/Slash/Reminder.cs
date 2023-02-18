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
    public async void AddReminder
    (
        InteractionContext ctx,
        [Option("timespan", "when the reminder should trigger")]
        TimeSpan? timeSpan,
        [Option("text","text")]
        string text
    )
    {
        await ctx.DeferAsync();
        
        var newReminder = new ReminderDbEntity()
        {
            UserId  = ctx.User.Id,
            ChannelId = ctx.Channel.Id,
            CreationTime = DateTime.UtcNow,
            ExecutionTime = DateTime.UtcNow + timeSpan.Value,
            ReminderText = text,
            IsPrivate = false
        };
        
        ReminderService.AddReminder(newReminder);
        
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("test"));
    }
}