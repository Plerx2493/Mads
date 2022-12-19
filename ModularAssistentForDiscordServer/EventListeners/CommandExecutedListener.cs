using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;

namespace MADS.EventListeners;

internal static partial class EventListener
{
    public static void AddCommandExecutedListener(ModularDiscordBot mdb)
    {
        var cnext = mdb.DiscordClient.GetCommandsNext();
        var slash = mdb.DiscordClient.GetSlashCommands();

        cnext.CommandExecuted += (sender, args) => CNextCommandExecuted(mdb, args);
        slash.SlashCommandExecuted += (sender, args) => SlashCommandExecuted(mdb, args);
        slash.ContextMenuExecuted += (sender, args) => ContextMenuExecuted(mdb, args);
    }

    private static async Task CNextCommandExecuted(ModularDiscordBot sender, CommandExecutionEventArgs eventArgs)
    {
    }

    private static async Task SlashCommandExecuted(ModularDiscordBot sender, SlashCommandExecutedEventArgs eventArgs)
    {
    }

    private static async Task ContextMenuExecuted(ModularDiscordBot sender, ContextMenuExecutedEventArgs eventArgs)
    {
    }
}