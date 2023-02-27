using Microsoft.EntityFrameworkCore;

namespace MADS.Entities;

public class MadsContext : DbContext
{
    public MadsContext(DbContextOptions<MadsContext> options) : base(options)
    {
    }

    public DbSet<UserDbEntity>             Users     { get; set; }
    public DbSet<GuildDbEntity>            Guilds    { get; set; }
    public DbSet<GuildConfigDbEntity>      Configs   { get; set; }
    public DbSet<IncidentDbEntity>         Incidents { get; set; }
    public DbSet<StarboardMessageDbEntity> Starboard { get; set; }
    public DbSet<QuoteDbEntity>            Quotes    { get; set; }
    public DbSet<ReminderDbEntity>         Reminders { get; set; } 

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(MadsContext).Assembly);

        base.OnModelCreating(builder);
    }
}