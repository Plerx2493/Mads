using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MADS.CustomComponents;

namespace MADS.Commands.Text.Base;

public class Jumppad : BaseCommandModule
{
    [Command("jumppad"), Aliases("jp"), RequireGuild, RequireUserPermissions(permissions: Permissions.MoveMembers)]
    public async Task Test(CommandContext ctx, ulong originChannel, ulong targetChannel)
    {
        DiscordMessageBuilder message = new();
        DiscordButtonComponent newButton = new(ButtonStyle.Success, "test", "Hüpf");
        var actionButton = ActionDiscordButton.Build(ActionDiscordButtonEnum.MoveVoiceChannel, newButton, originChannel,
            targetChannel);

        message.AddComponents(actionButton);
        message.Content = "Jumppad";
        await ctx.RespondAsync(message);
    }
}