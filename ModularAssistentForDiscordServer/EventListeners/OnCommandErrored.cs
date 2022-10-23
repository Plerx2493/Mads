using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;

namespace MADS.EventListeners;

public static partial class EventListener 
{
    public static async Task OnSlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
    {
        var typeOfException = e.Exception.GetType();
        if (typeOfException == typeof(ArgumentException)
            || typeOfException == typeof(SlashExecutionChecksFailedException))
        {
            return;
        }

        DiscordEmbedBuilder discordEmbed = new()
        {
            Title = $"{Formatter.Bold("Error")} - The command execution failed",
            Description = (e.Exception.Message + ":\n" + e.Exception.StackTrace).Take(4096).ToString(),
            Color = DiscordColor.Red,
            Timestamp = DateTime.Now
        };
        

        await e.Context.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AddEmbed(discordEmbed).AsEphemeral());
    }

    public static async Task OnCNextErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
    {
        var typeOfException = e.Exception.GetType();
        if (typeOfException == typeof(ChecksFailedException) || typeOfException == typeof(ArgumentException)
                                                             || typeOfException == typeof(CommandNotFoundException))
        {
            return;
        }

        await e.Context.Message.RespondAsync($"OOPS your command just errored... \n {e.Exception.Message}");
        await e.Context.Message.RespondAsync(e.Exception.InnerException?.Message ?? "no inner exception");
        
        var reallyLongString = e.Exception.StackTrace;

        var interactivity = e.Context.Client.GetInteractivity();
        var pages = interactivity.GeneratePagesInEmbed(reallyLongString);

        await e.Context.Channel.SendPaginatedMessageAsync(e.Context.Member, pages, PaginationBehaviour.Ignore, ButtonPaginationBehavior.DeleteButtons);
    }
}