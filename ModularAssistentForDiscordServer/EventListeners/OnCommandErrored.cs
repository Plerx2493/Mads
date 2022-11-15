using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Humanizer;

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

        var embedDescription = new string((e.Exception.Message + ":\n" + e.Exception.StackTrace).Take(4096).ToArray());
        
        DiscordEmbedBuilder discordEmbed = new()
        {
            Title = $"{Formatter.Bold(e.Exception.GetType().ToString())} - The command execution failed",
            Description = Formatter.BlockCode(embedDescription, "cs"),
            Color = DiscordColor.Red,
            Timestamp = DateTime.Now
        };

        try
        {
            await e.Context.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(discordEmbed).AsEphemeral());
        }
        catch(BadRequestException)
        {
            await e.Context.Channel.SendMessageAsync(discordEmbed);
        }
       
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