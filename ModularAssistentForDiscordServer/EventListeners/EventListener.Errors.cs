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

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;

namespace MADS.EventListeners;

internal static partial class EventListener
{
    internal static async Task OnSlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
    {
        Type typeOfException = e.Exception.GetType();
        if (typeOfException == typeof(ArgumentException)
            || typeOfException == typeof(SlashExecutionChecksFailedException))
        {
            return;
        }

        string embedDescription = new((e.Exception.Message + ":\n" + e.Exception.StackTrace).Take(4093).ToArray());
        embedDescription += "...";

        DiscordEmbedBuilder discordEmbed = new()
        {
            Title = $"{Formatter.Bold(e.Exception.GetType().ToString())} - The command execution failed",
            Description = Formatter.BlockCode(embedDescription, "cs"),
            Color = DiscordColor.Red,
            Timestamp = DateTime.Now
        };

        try
        {
            await e.Context.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AddEmbed(discordEmbed).AsEphemeral());
            return;
        }
        catch (BadRequestException)
        {
        }

        try
        {
            await e.Context.Interaction.EditOriginalResponseAsync(
                new DiscordWebhookBuilder(new DiscordInteractionResponseBuilder().AddEmbed(discordEmbed)
                    .AsEphemeral()));
            return;
        }
        catch (BadRequestException)
        {
        }

        await e.Context.Channel.SendMessageAsync(discordEmbed);
    }

    internal static async Task OnCNextErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
    {
        Type typeOfException = e.Exception.GetType();
        if (typeOfException == typeof(ChecksFailedException) || typeOfException == typeof(ArgumentException)
                                                             || typeOfException == typeof(CommandNotFoundException))
        {
            return;
        }

        await e.Context.Message.RespondAsync($"OOPS your command just errored... \n {e.Exception.Message}");
        await e.Context.Message.RespondAsync(e.Exception.InnerException?.Message ?? "no inner exception");
    }

    internal static async Task OnClientErrored(DiscordClient sender, ClientErrorEventArgs e)
    {
        DiscordEmbedBuilder exceptionEmbed = new DiscordEmbedBuilder()
            .WithAuthor("Mads-Debug")
            .WithColor(new DiscordColor(0, 255, 194))
            .WithTimestamp(DateTime.UtcNow)
            .WithTitle($"Eventhandler exception: {e.EventName} - {e.Exception.GetType()}")
            .WithDescription(e.Exception.Message);
        
        if (!string.IsNullOrWhiteSpace(e.Exception.StackTrace))
        {
            if (e.Exception.StackTrace.Length > 1013)
            {
                string stacktrace = e.Exception.StackTrace[..1013] + "...";
                exceptionEmbed.AddField("Stacktrace", Formatter.BlockCode(stacktrace));
            }
            else
            {
                exceptionEmbed.AddField("Stacktrace", Formatter.BlockCode(e.Exception.StackTrace));
            }
        }

        DiscordWebhookBuilder webhookBuilder = new DiscordWebhookBuilder()
            .WithUsername("Mads-Debug")
            .AddEmbed(exceptionEmbed);

        await MainProgram.WebhookClient.BroadcastMessageAsync(webhookBuilder);
    }

    internal static async Task OnAutocompleteError(SlashCommandsExtension sender, AutocompleteErrorEventArgs e)
    {
        await e.Context.Channel.SendMessageAsync($"OOPS your command just errored... \n {e.Exception.Message}");
        await e.Context.Channel.SendMessageAsync(e.Exception.InnerException?.Message ?? "no inner exception");
        string? reallyLongString = e.Exception.StackTrace;

        InteractivityExtension interactivity = e.Context.Client.GetInteractivity();
        if (reallyLongString != null)
        {
            IEnumerable<Page> pages = interactivity.GeneratePagesInEmbed(reallyLongString);

            await e.Context.Channel.SendPaginatedMessageAsync(e.Context.Member, pages, PaginationBehaviour.Ignore,
                ButtonPaginationBehavior.DeleteButtons);
        }
    }
    
    internal static async Task OnSocketErrored(DiscordClient client, SocketErrorEventArgs e)
    {
        DiscordEmbedBuilder exceptionEmbed = new DiscordEmbedBuilder()
            .WithAuthor("Mads-Debug")
            .WithColor(new DiscordColor(0, 255, 194))
            .WithTimestamp(DateTime.UtcNow)
            .WithTitle($"Gateway socket exception: {e.Exception.GetType()} - ")
            .WithDescription(e.Exception.Message);
        
        if (!string.IsNullOrWhiteSpace(e.Exception.StackTrace))
        {
            if (e.Exception.StackTrace.Length > 1013)
            {
                string stacktrace = e.Exception.StackTrace[..1013] + "...";
                exceptionEmbed.AddField("Stacktrace", Formatter.BlockCode(stacktrace));
            }
            else
            {
                exceptionEmbed.AddField("Stacktrace", Formatter.BlockCode(e.Exception.StackTrace));
            }
        }
        
        DiscordWebhookBuilder webhookBuilder = new DiscordWebhookBuilder()
            .WithUsername("Mads-Debug")
            .AddEmbed(exceptionEmbed);
        
        await MainProgram.WebhookClient.BroadcastMessageAsync(webhookBuilder);
    }
}