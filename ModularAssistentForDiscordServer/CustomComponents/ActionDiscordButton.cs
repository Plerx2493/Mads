using DSharpPlus.Entities;

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

    
}