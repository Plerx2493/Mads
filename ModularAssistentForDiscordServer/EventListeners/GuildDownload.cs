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
using DSharpPlus.EventArgs;
using MADS.Entities;
using Microsoft.EntityFrameworkCore;

namespace MADS.EventListeners;

internal static partial class EventListener
{
    public static void GuildDownload(DiscordClient client, IDbContextFactory<MadsContext> contextFactory)
    {
        client.GuildDownloadCompleted += async (sender, args) =>
        {
            await UpdateDb(args, contextFactory);
        };
    }

    private static async Task UpdateDb
    (
        GuildDownloadCompletedEventArgs args, 
        IDbContextFactory<MadsContext> dbFactory
    )
    {
        await UpdateGuilds(args, dbFactory);
        await UpdateUsersDb(args, dbFactory);
    }

    private static async Task UpdateUsersDb
    (
        GuildDownloadCompletedEventArgs args,
        IDbContextFactory<MadsContext> dbFactory
    )
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var newUserIds = args.Guilds.Values
                             .SelectMany(x => x.Members.Values)
                             .Select(x => x.Id)
                             .Distinct()
                             .Except(db.Users.Select(y => y.Id));

        var newUserDbEntities = newUserIds.Select(userId =>
        {
            var user = args.Guilds.Values
                           .SelectMany(x => x.Members.Values)
                           .First(x => x.Id == userId);

            return new UserDbEntity()
            {
                Id = user.Id,
                Username = user.Username,
                Discriminator = Convert.ToInt32(user.Discriminator)
            };
        });

        await db.Users.AddRangeAsync(newUserDbEntities);
        await db.SaveChangesAsync();
    }


    private static async Task UpdateGuilds
    (
        GuildDownloadCompletedEventArgs args,
        IDbContextFactory<MadsContext> dbFactory
    )
    {
        await using var db = await dbFactory.CreateDbContextAsync();

        var dbGuildIds = db.Guilds.Select(x => x.DiscordId).ToList();

        var newGuildEntities = args.Guilds
                                   .Where(x => !dbGuildIds.Contains(x.Key))
                                   .Select(x => new GuildDbEntity
                                   {
                                       DiscordId = x.Value.Id,
                                       Settings = new GuildConfigDbEntity
                                       {
                                           DiscordGuildId = x.Value.Id,
                                           Prefix = "",
                                           StarboardActive = false
                                       }
                                   });

        await db.Guilds.AddRangeAsync(newGuildEntities);
        await db.SaveChangesAsync();
    }
}