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
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Humanizer;

namespace MADS.Commands.ContextMenu;

public class UserInfoUser
{
    [SlashCommandTypes(DiscordApplicationCommandType.UserContextMenu), Command("Info")]
    public async Task GetUserInfo(SlashCommandContext ctx, DiscordUser targetUser)
    {
        DiscordUser user = targetUser;

        DiscordMember? member = null;
        
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

        string userUrl = "https://discordapp.com/users/" + user.Id;

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
                embed.AddField("2FA:", member.MfaEnabled.ToString()!);
            }

            embed.AddField("Permissions:", member.Permissions.Humanize());

            embed.AddField("Hierarchy:",
                member.Hierarchy != int.MaxValue ? member.Hierarchy.ToString() : "Server owner", true);


            if (member.Roles.Any())
            {
                embed.AddField("Roles", member.Roles.Select(x => x.Name).Humanize());
            }
        }

        await ctx.RespondAsync(embed.Build(), true);
    }
}