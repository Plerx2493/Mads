using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace MADS.CustomComponents;

public static class ActionDiscordButton
{
    public static DiscordButtonComponent Build(ActionDiscordButtonEnum action, DiscordButtonComponent button,
        params object[] args)
    {
        int actionCode;

        var customId = "CMD:";
        switch (action)
        {
            case ActionDiscordButtonEnum.BanUser:
                if (args.Length != 1)
                {
                    throw new ArgumentException("Only one id possible");
                }
                if (args[0].GetType() != typeof(ulong))
                {
                    throw new ArgumentException("Please provide a id (ulong)");
                }
                actionCode = (int)ActionDiscordButtonEnum.BanUser;
                customId += actionCode + ":" + args[0];
                break;

            case ActionDiscordButtonEnum.KickUser:
                if (args.Length != 1)
                {
                    throw new ArgumentException("Only one id possible");
                }
                if (args[0].GetType() != typeof(ulong))
                {
                    throw new ArgumentException("Please provide a id (ulong)");
                }
                actionCode = (int)ActionDiscordButtonEnum.BanUser;
                customId += actionCode + ":" + args[0];
                break;

            case ActionDiscordButtonEnum.GetIdUser:
                if (args.Length != 1)
                {
                    throw new ArgumentException("Only one id possible");
                }
                if (args[0].GetType() != typeof(ulong))
                {
                    throw new ArgumentException("Please provide a id (ulong)");
                }
                actionCode = (int)ActionDiscordButtonEnum.GetIdUser;
                customId += actionCode + ":" + args[0];
                break;

            case ActionDiscordButtonEnum.GetIdGuild:
                if (args.Length != 1)
                {
                    throw new ArgumentException("Only one id possible");
                }
                if (args[0].GetType() != typeof(ulong))
                {
                    throw new ArgumentException("Please provide a id (ulong)");
                }
                actionCode = (int)ActionDiscordButtonEnum.GetIdGuild;
                customId += actionCode + ":" + args[0];
                break;

            case ActionDiscordButtonEnum.GetIdChannel:
                if (args.Length != 1)
                {
                    throw new ArgumentException("Only one id possible");
                }
                if (args[0].GetType() != typeof(ulong))
                {
                    throw new ArgumentException("Please provide a id (ulong)");
                }
                actionCode = (int)ActionDiscordButtonEnum.GetIdChannel;
                customId += actionCode + ":" + args[0];
                break;

            case ActionDiscordButtonEnum.MoveVoiceChannel:
                if (args.Length != 2)
                {
                    throw new ArgumentException("Only 2 ids possible");
                }
                if (args[0].GetType() != typeof(ulong))
                {
                    throw new ArgumentException("Please provide 2 ids (ulong)");
                }
                if (args[1].GetType() != typeof(ulong))
                {
                    throw new ArgumentException("Please provide 2 ids (ulong)");
                }
                actionCode = (int)ActionDiscordButtonEnum.MoveVoiceChannel;
                customId += actionCode + ":" + args[0] + ":" + args[1];
                break;

            default:
                throw new NotImplementedException("Action code not implemented");
        }

        var label = button.Label;
        var style = button.Style;
        var commandButton = new DiscordButtonComponent(style, customId, label);

        return commandButton;
    }

    public static void EnableButtonListener(DiscordClient client)
    {
        client.ComponentInteractionCreated += async (s, e) =>
        {
            if (!Regex.IsMatch(e.Id, @"^CMD:\d{4}(?::\d{1,20}){0,3}$"))
            {
                return;
            }

            var substring = e.Id.Split(':');
            if (!int.TryParse(substring[1], out var actionCode))
            {
                return;
            }

            substring = substring.Skip(1).ToArray();

            switch (actionCode)
            {
                case (int)ActionDiscordButtonEnum.BanUser:
                    BanUser(e, substring);
                    break;

                case (int)ActionDiscordButtonEnum.KickUser:
                    KickUser(e, substring);
                    break;

                case (int)ActionDiscordButtonEnum.GetIdUser:
                    GetUserId(e, substring);
                    break;

                case (int)ActionDiscordButtonEnum.GetIdGuild:
                    GetGuildId(e, substring);
                    break;

                case (int)ActionDiscordButtonEnum.GetIdChannel:
                    GetChannelId(e, substring);
                    break;

                case (int)ActionDiscordButtonEnum.MoveVoiceChannel:
                    await MoveVoiceChannelUser(e, substring);
                    break;

                default:
                    throw new NotImplementedException("Action code not implemented");
            }
        };
    }

    private static async Task MoveVoiceChannelUser(ComponentInteractionCreateEventArgs e,
        IReadOnlyList<string> substring)
    {
        var member = await e.Guild.GetMemberAsync(e.User.Id);
        if (!member.Permissions.HasPermission(Permissions.MoveMembers)) { return; }
        var originChannel = e.Guild.GetChannel(ulong.Parse(substring[1]));
        var targetChannel = e.Guild.GetChannel(ulong.Parse(substring[2]));

        foreach (var voiceMember in originChannel.Users)
        {
            await targetChannel.PlaceMemberAsync(voiceMember);
        }
        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource);
    }

    private static async void BanUser(ComponentInteractionCreateEventArgs e, IReadOnlyList<string> substring)
    {
        var member = await e.Guild.GetMemberAsync(e.User.Id);
        if (!member.Permissions.HasPermission(Permissions.BanMembers)) { return; }

        var userId = ulong.Parse(substring[1]);
        await e.Guild.BanMemberAsync(userId);
        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource);
    }

    private static async void KickUser(ComponentInteractionCreateEventArgs e, IReadOnlyList<string> substring)
    {
        var member = await e.Guild.GetMemberAsync(e.User.Id);
        if (!member.Permissions.HasPermission(Permissions.KickMembers)) { return; }

        var userId = ulong.Parse(substring[1]);
        await e.Guild.BanMemberAsync(userId);
        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource);
    }

    private static async void GetUserId(InteractionCreateEventArgs e, IReadOnlyList<string> substring)
    {
        var response = new DiscordInteractionResponseBuilder();

        response.WithContent("User id: " + ulong.Parse(substring[1]));
        response.AsEphemeral();

        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
    }

    private static async void GetGuildId(InteractionCreateEventArgs e, IReadOnlyList<string> substring)
    {
        var response = new DiscordInteractionResponseBuilder();

        response.WithContent("Guild id: " + ulong.Parse(substring[1]));
        response.AsEphemeral();

        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
    }

    private static async void GetChannelId(InteractionCreateEventArgs e, IReadOnlyList<string> substring)
    {
        var response = new DiscordInteractionResponseBuilder();

        response.WithContent("Channel id: " + ulong.Parse(substring[1]));
        response.AsEphemeral();

        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
    }
}