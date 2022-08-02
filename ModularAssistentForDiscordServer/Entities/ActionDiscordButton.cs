using DSharpPlus.Entities;
using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MADS.Entities
{
    public static class ActionDiscordButton
    {
        public static DiscordButtonComponent Build(ActionDiscordButtonEnum action, DiscordButtonComponent button, params object[] args)
        {
            string customId = "";
            int actioncode = 0;


            switch (action)
            {
                case ActionDiscordButtonEnum.BanUser:
                    if (args.Length != 1) throw new ArgumentException("Only one id possible");
                    if (args[0].GetType() != typeof(ulong)) throw new ArgumentException("Please provide a id (ulong)");
                    actioncode = (int)ActionDiscordButtonEnum.BanUser;
                    customId = actioncode + ":" + args[0];
                    break;

                case ActionDiscordButtonEnum.KickUser:
                    if (args.Length != 1) throw new ArgumentException("Only one id possible");
                    if (args[0].GetType() != typeof(ulong)) throw new ArgumentException("Please provide a id (ulong)");
                    actioncode = (int)ActionDiscordButtonEnum.BanUser;
                    customId = actioncode + ":" + args[0];
                    break;


                default:
                    break;
            }

            if (customId == "") throw new Exception();


            string lable = button.Label;
            var style = button.Style;
            var CommandButton = new DiscordButtonComponent(style, customId, lable);

            return CommandButton;
        }

        public static void EnableButtonListener(DiscordClient client)
        {
            client.ComponentInteractionCreated += async (s, e) =>
            {
                var substring = e.Id.Split(':');
                if (!int.TryParse(substring[0], out int actionCode))
                {
                    return;
                }

                e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource);

                ulong userID;
                switch (actionCode)
                {
                    case (int)ActionDiscordButtonEnum.BanUser:
                        userID = ulong.Parse(substring[1]);
                        await e.Guild.BanMemberAsync(userID);
                        break;

                    case (int)ActionDiscordButtonEnum.KickUser:
                        userID = ulong.Parse(substring[1]);
                        await e.Guild.BanMemberAsync(userID);
                        break;
                }
            };
        }
    }
}
