using DSharpPlus;
using DSharpPlus.EventArgs;
using MADS.Entities;
using Microsoft.EntityFrameworkCore;

namespace MADS.EventListeners;

internal static partial class EventListener
{
    public static void GuildDownload(DiscordClient client, IDbContextFactory<MadsContext> contextFactory)
    {
        client.GuildDownloadCompleted += (sender, args) =>
        {
            _ = Task.Run(() => UpdateDb(args, contextFactory));
            return Task.CompletedTask;
        };
    }

    private static async Task UpdateDb
        (GuildDownloadCompletedEventArgs args, IDbContextFactory<MadsContext> dbFactory)
    {
        await UpdateGuilds(args, dbFactory);
        await UpdateUsersDb(args, dbFactory);
    }

    private static async Task UpdateUsersDb(GuildDownloadCompletedEventArgs args,
        IDbContextFactory<MadsContext> dbFactory)
    {
        await using var db = await dbFactory.CreateDbContextAsync();
        var newUserIds = args.Guilds.Values
                             .SelectMany(x => x.Members.Values)
                             .Select(x => x.Id)
                             .Distinct()
                             .Except(db.Users.Select(y => y.Id))
                             .ToList();

        var newUserDbEntities = newUserIds.Select(userId =>
        {
            var user = args.Guilds.Values
                           .SelectMany(x => x.Members.Values)
                           .FirstOrDefault(x => x.Id == userId);

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


    private static async Task UpdateGuilds(GuildDownloadCompletedEventArgs args,
        IDbContextFactory<MadsContext> dbFactory)
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