using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Humanizer;
using MADS.Entities;
using Microsoft.EntityFrameworkCore;

namespace MADS.Commands.Text.Base;

public class UserInfo : BaseCommandModule
{
    public MadsServiceProvider CommandService { get; set; }
    public IDbContextFactory<MadsContext> DbFactory { get; set; }

    [Command("user"), Aliases("userinfo", "stalking")]
    public async Task GetUserInfo(CommandContext ctx, DiscordUser user = null)
    {
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

        embed
            .WithAuthor($"{user.Username}#{user.Discriminator}", null, user.AvatarUrl)
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

            embed
                .AddField("Permissions:", member.Permissions.Humanize())
                .AddField("Hierarchy:", member.Hierarchy.ToString(), true);
            if (member.Roles.Any())
            {
                embed.AddField("Roles", member.Roles.Select(x => x.Name).Humanize());
            }
        }

        await ctx.RespondAsync(embed.Build());
    }
}