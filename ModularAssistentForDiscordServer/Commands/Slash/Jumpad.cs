using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MADS.CustomComponents;

namespace MADS.Commands.Slash;

[GuildOnly]
public class Jumppad : ApplicationCommandModule
{
    [SlashCommand("jumppad", "Create a jumppad button"), SlashCommandPermissions(Permissions.MoveMembers)]
    public async Task Test(InteractionContext ctx,
        [Option("originChannel", "Channel where the users will be moved out"), ChannelTypes(ChannelType.Voice)] 
        DiscordChannel originChannel,
        [Option("originChannel", "Channel where the users will be put in"), ChannelTypes(ChannelType.Voice)]
        DiscordChannel targetChannel
        )
    {
        DiscordInteractionResponseBuilder message = new();
        DiscordButtonComponent newButton = new(ButtonStyle.Success, "Placeholder", "Jump");
        var actionButton = ActionDiscordButton.Build(ActionDiscordButtonEnum.MoveVoiceChannel, newButton, originChannel.Id,
            targetChannel.Id);

        message.AddComponents(actionButton);
        message.Content = "Jumppad";
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, message);
    }
}