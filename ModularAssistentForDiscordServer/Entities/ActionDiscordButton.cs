using DSharpPlus.Entities;
using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;

namespace MADS.Entities
{
    public static class ActionDiscordButton
    {
        public static DiscordButtonComponent Build(ActionDiscordButtonEnum action, DiscordButtonComponent button, params object[] args)
        {
            int actioncode;

            string customId;
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

                case ActionDiscordButtonEnum.GetIDUser:
                    if (args.Length != 1) throw new ArgumentException("Only one id possible");
                    if (args[0].GetType() != typeof(ulong)) throw new ArgumentException("Please provide a id (ulong)");
                    actioncode = (int)ActionDiscordButtonEnum.GetIDUser;
                    customId = actioncode + ":" + args[0];
                    break;

                case ActionDiscordButtonEnum.GetIDGuild:
                    if (args.Length != 1) throw new ArgumentException("Only one id possible");
                    if (args[0].GetType() != typeof(ulong)) throw new ArgumentException("Please provide a id (ulong)");
                    actioncode = (int)ActionDiscordButtonEnum.GetIDGuild;
                    customId = actioncode + ":" + args[0];
                    break;

                case ActionDiscordButtonEnum.GetIDChannel:
                    if (args.Length != 1) throw new ArgumentException("Only one id possible");
                    if (args[0].GetType() != typeof(ulong)) throw new ArgumentException("Please provide a id (ulong)");
                    actioncode = (int)ActionDiscordButtonEnum.GetIDChannel;
                    customId = actioncode + ":" + args[0];
                    break;

                case ActionDiscordButtonEnum.MoveVoiceChannel:
                    if (args.Length != 2) throw new ArgumentException("Only 2 ids possible");
                    if (args[0].GetType() != typeof(ulong)) throw new ArgumentException("Please provide 2 ids (ulong)");
                    if (args[1].GetType() != typeof(ulong)) throw new ArgumentException("Please provide 2 ids (ulong)");
                    actioncode = (int)ActionDiscordButtonEnum.MoveVoiceChannel;
                    customId = actioncode + ":" + args[0] + ":" + args[1];
                    break;

                default:
                    throw new NotImplementedException("Action code not implemented");
                    break;
            }

            if (customId is null) throw new Exception();

            string lable = button.Label;
            var style = button.Style;
            var CommandButton = new DiscordButtonComponent(style, customId, lable);

            return CommandButton;
        }

        public static void EnableButtonListener(DiscordClient client)
        {
            client.ComponentInteractionCreated += (s, e) =>
            {
                var substring = e.Id.Split(':');
                if (!int.TryParse(substring[0], out int actionCode))
                {
                    return null;
                }

                switch (actionCode)
                {
                    case (int)ActionDiscordButtonEnum.BanUser:
                        BanUser(e, substring);
                        break;

                    case (int)ActionDiscordButtonEnum.KickUser:
                        KickUser(e, substring);
                        break;

                    case (int)ActionDiscordButtonEnum.GetIDUser:
                        GetUserID(e, substring);
                        break;

                    case (int)ActionDiscordButtonEnum.GetIDGuild:
                        GetGuildID(e, substring);
                        break;

                    case (int)ActionDiscordButtonEnum.GetIDChannel:
                        GetUserID(e, substring);
                        break;

                    case (int)ActionDiscordButtonEnum.MoveVoiceChannel:
                        MoveVoiceChannelUser(e, substring);
                        break;


                    default:
                        throw new NotImplementedException("Action code not implemented");
                        break;
                }
                return null;
            };
        }

        private static void MoveVoiceChannelUser(ComponentInteractionCreateEventArgs e, string[] substring)
        {
            DiscordChannel originChannel = e.Guild.GetChannel(ulong.Parse(substring[1]));
            DiscordChannel targetChannel = e.Guild.GetChannel(ulong.Parse(substring[2]));

            foreach (DiscordMember member in originChannel.Users)
            {
                targetChannel.PlaceMemberAsync(member);
            }
            e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource);
        }

        private static async void BanUser(ComponentInteractionCreateEventArgs e, string[] substring)
        {
            var member = await e.Guild.GetMemberAsync(e.User.Id);
            var memberPermissions = member.Permissions;
            var neededPermissions = Permissions.BanMembers;

            if (!PermissionMethods.HasPermission(memberPermissions, neededPermissions)) { return; }
            
            ulong userID = ulong.Parse(substring[1]);
            await e.Guild.BanMemberAsync(userID);
            e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource);
        }

        private static async void KickUser(ComponentInteractionCreateEventArgs e, string[] substring)
        {
            var member = await e.Guild.GetMemberAsync(e.User.Id);
            var memberPermissions = member.Permissions;
            var neededPermissions = Permissions.KickMembers;

            if (!PermissionMethods.HasPermission(memberPermissions, neededPermissions)) { return; }

            ulong userID = ulong.Parse(substring[1]);
            await e.Guild.BanMemberAsync(userID);
            e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource);
        }

        private static async void GetUserID(ComponentInteractionCreateEventArgs e, string[] substring)
        {
            var response = new DiscordInteractionResponseBuilder();

            response.WithContent("User id: " + ulong.Parse(substring[1]));
            response.AsEphemeral(true);

            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
        }

        private static async void GetGuildID(ComponentInteractionCreateEventArgs e, string[] substring)
        {
            var response = new DiscordInteractionResponseBuilder();

            response.WithContent("Guild id: " + ulong.Parse(substring[1]));
            response.AsEphemeral(true);

            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
        }

        private static async void GetChannelID(ComponentInteractionCreateEventArgs e, string[] substring)
        {
            var response = new DiscordInteractionResponseBuilder();

            response.WithContent("Channel id: " + ulong.Parse(substring[1]));
            response.AsEphemeral(true);

            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
        }
    }
}
