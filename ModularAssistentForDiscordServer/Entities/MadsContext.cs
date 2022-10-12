using Microsoft.EntityFrameworkCore;

namespace MADS.Entities;

public class MadsContext : DbContext
{
    public MadsContext(DbContextOptions<MadsContext> options) : base(options) { }

    public DbSet<UserDbEntity> Users { get; set; }
    public DbSet<GuildDbEntity> Guilds { get; set; }
    public DbSet<GuildUserDbEntity> GuildUsers { get; set; }
    public DbSet<IncidentDbEntity> Incidents { get; set; }
    public DbSet<GuildConfigDbEntity> GuildConfigs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(MadsContext).Assembly);

        base.OnModelCreating(builder);
    }
}