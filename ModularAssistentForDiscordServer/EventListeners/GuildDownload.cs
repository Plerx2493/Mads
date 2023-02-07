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
            
            var _ = Task.Run(() => GuildConfigCheck(sender, args, contextFactory));
            return Task.CompletedTask;
        };
    }

    private static async Task GuildConfigCheck
        (DiscordClient sender, GuildDownloadCompletedEventArgs args, IDbContextFactory<MadsContext> dbFactory)
    {
        var db = await dbFactory.CreateDbContextAsync();
        
        var dbGuilds = db.Guilds.Select(x => x.DiscordId).ToList();

        var newGuilds = args.Guilds.Where(x => !dbGuilds.Contains(x.Key)).Select(x => x.Value).ToList();

        var newGuildEntities = new List<GuildDbEntity>();
        var newGuildConfigEntities = new List<GuildConfigDbEntity>();
        
        foreach (var guild in newGuilds)
        {
            var settings = new GuildConfigDbEntity
            {
                DiscordGuildId = guild.Id,
                Prefix = "",
                StarboardActive = false
            };
            
            var newConfig = new GuildDbEntity
            {
                Settings = settings,
                DiscordId = guild.Id
            };
            
            newGuildConfigEntities.Add(settings);
            newGuildEntities.Add(newConfig);
        }
        
        await db.Guilds.AddRangeAsync(newGuildEntities);
        await db.SaveChangesAsync();
        
        Console.WriteLine(db.Guilds.Count());
        
        await db.DisposeAsync();
    }
}