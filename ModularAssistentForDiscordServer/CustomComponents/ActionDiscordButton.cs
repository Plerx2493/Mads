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
using DSharpPlus.Entities;

namespace MADS.CustomComponents;

public static class ActionDiscordButton
{
    public static DiscordButtonComponent AsActionButton
    (
        this DiscordButtonComponent button,
        ActionDiscordButtonEnum action,
        params object[] args
    )
    {
        int actionCode;

        string customId = "CMD:";
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

                actionCode = (int) ActionDiscordButtonEnum.BanUser;
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

                actionCode = (int) ActionDiscordButtonEnum.BanUser;
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

                actionCode = (int) ActionDiscordButtonEnum.GetIdUser;
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

                actionCode = (int) ActionDiscordButtonEnum.GetIdGuild;
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

                actionCode = (int) ActionDiscordButtonEnum.GetIdChannel;
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

                actionCode = (int) ActionDiscordButtonEnum.MoveVoiceChannel;
                customId += actionCode + ":" + args[0] + ":" + args[1];
                break;

            case ActionDiscordButtonEnum.DeleteOneUserOnly:
                if (args.Length != 1)
                {
                    throw new ArgumentException("Only 1 id possible");
                }

                if (args[0].GetType() != typeof(ulong))
                {
                    throw new ArgumentException("Please provide 1 id (ulong)");
                }

                actionCode = (int) ActionDiscordButtonEnum.DeleteOneUserOnly;
                customId += actionCode + ":" + args[0];
                break;

            case ActionDiscordButtonEnum.AnswerDmChannel:
                if (args.Length != 1)
                {
                    throw new ArgumentException("Only 1 id possible");
                }

                if (args[0].GetType() != typeof(ulong))
                {
                    throw new ArgumentException("Please provide 1 id (ulong)");
                }

                actionCode = (int) ActionDiscordButtonEnum.AnswerDmChannel;
                customId += actionCode + ":" + args[0];
                break;

            default:
                throw new NotImplementedException("Action code not implemented");
        }

        string label = button.Label;
        ButtonStyle style = button.Style;
        DiscordButtonComponent commandButton = new(style, customId, label);

        return commandButton;
    }
}