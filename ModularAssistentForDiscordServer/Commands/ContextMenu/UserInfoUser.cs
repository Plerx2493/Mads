using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using Humanizer;
using MADS.Extensions;

namespace MADS.Commands.ContextMenu;

public class UserInfoUser : MadsBaseApplicationCommand
{

    [ContextMenu(ApplicationCommandType.UserContextMenu, "Info")]
    public async Task GetUserInfo(ContextMenuContext ctx)
    {
        //await ctx.CreateResponseAsync("Test", true);

        var user = ctx.TargetUser;

        DiscordMember member = null;

        user ??= ctx.User;
        try
        {
            if (!ctx.Channel.IsPrivate)
            {
                member = await ctx.Guild.GetMemberAsync(user.Id);
            }
        }
        catch (DiscordException e)
        {
            if (e.GetType() != typeof(NotFoundException))
            {
                throw;
            }
        }

        DiscordEmbedBuilder embed = new();

        var userUrl = "https://discordapp.com/users/" + user.Id;

        embed
            .WithAuthor($"{user.Username}#{user.Discriminator}", userUrl, user.AvatarUrl)
            .WithColor(new DiscordColor(0, 255, 194))
            .AddField("Creation:",
                $"{user.CreationTimestamp.Humanize()} {Formatter.Timestamp(user.CreationTimestamp, TimestampFormat.ShortDate)}",
                true)
            .AddField("ID:", user.Id.ToString(), true);

        if (member is not null)
        {
            embed.AddField("Joined at:",
                $"{member.JoinedAt.Humanize()} {Formatter.Timestamp(member.JoinedAt, TimestampFormat.ShortDate)}",
                true);
            if (member.MfaEnabled.HasValue)
            {
                embed.AddField("2FA:", member.MfaEnabled.ToString());
            }

            embed.AddField("Permissions:", member.Permissions.Humanize());

            embed.AddField("Hierarchy:",
                member.Hierarchy != int.MaxValue ? member.Hierarchy.ToString() : "Server owner", true);


            if (member.Roles.Any())
            {
                embed.AddField("Roles", member.Roles.Select(x => x.Name).Humanize());
            }
        }

        await ctx.CreateResponseAsync(embed.Build(), true);
    }
}
