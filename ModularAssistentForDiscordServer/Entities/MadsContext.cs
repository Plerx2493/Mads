using System.Data.Entity.ModelConfiguration.Configuration;
using Microsoft.EntityFrameworkCore;

namespace MADS.Entities;

public class MadsContext : DbContext
{
    public MadsContext(DbContextOptions<MadsContext> options) : base(options) { }
    
    public DbSet<UserDbEntity>        Users        { get; set; }
    public DbSet<GuildDbEntity>       Guilds       { get; set; }
    public DbSet<GuildUserDbEntity>   GuildUsers   { get; set; }
    public DbSet<IncidentDbEntity>    Incidents    { get; set; }
    public DbSet<GuildConfigDbEntity> GuildConfigs { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        var connectionString = "Server=192.168.178.61,Port=3306;Database=MadsDB;User=USR;Password=PWD;";
        optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(MadsContext).Assembly);
        
        base.OnModelCreating(builder);
    }
}