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
using DSharpPlus.EventArgs;
using MADS.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MADS.EventListeners;

internal static partial class EventListener
{
    public static async Task UpdateDb(DiscordClient client, GuildDownloadCompletedEventArgs args)
    {
        IDbContextFactory<MadsContext> dbFactory = ModularDiscordBot.Services.GetRequiredService<IDbContextFactory<MadsContext>>();
        Task updateGuilds = UpdateGuilds(args, dbFactory);
        Task updateUsers = UpdateUsersDb(args, dbFactory);
        
        await Task.WhenAll(updateGuilds, updateUsers);
        client.Logger.LogInformation("Database updated!");
    }

    private static async Task UpdateUsersDb
    (
        GuildDownloadCompletedEventArgs args,
        IDbContextFactory<MadsContext> dbFactory
    )
    {
        await using MadsContext db = await dbFactory.CreateDbContextAsync();
        List<ulong> oldDbUsers = db.Users.Select(x => x.Id).ToList();
        
        ulong[] newUserIds = args.Guilds.Values
            .SelectMany(x => x.Members.Values)
            .Select(x => x.Id)
            .Distinct()
            .Except(oldDbUsers)
            .ToArray();

        DiscordMember[] users = args.Guilds.Values
            .SelectMany(x => x.Members.Values)
            .ToArray();

        List<UserDbEntity> userDbEntities = new(newUserIds.Length);

        foreach (ulong userId in newUserIds)
        {
            DiscordMember? user = users.FirstOrDefault(x => x.Id == userId);
                
            if (user is null)
            {
                continue;
            }

            UserDbEntity dbEntity = new()
            {
                Id = user.Id,
                Username = user.Username,
                PreferedLanguage = "en-US"
            };
            
            userDbEntities.Add(dbEntity);
        }
        
        await db.Users.AddRangeAsync(userDbEntities);
        await db.SaveChangesAsync();
    }


    private static async Task UpdateGuilds
    (
        GuildDownloadCompletedEventArgs args,
        IDbContextFactory<MadsContext> dbFactory
    )
    {
        await using MadsContext db = await dbFactory.CreateDbContextAsync();

        List<ulong> dbGuildIds = db.Guilds.Select(x => x.DiscordId).ToList();

        GuildDbEntity[] newGuildEntities = args.Guilds
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
            })
            .ToArray();

        await db.Guilds.AddRangeAsync(newGuildEntities);
        await db.SaveChangesAsync();
    }
}