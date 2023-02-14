using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MADS.Entities.Configs;

public class GuildEntityConfig : IEntityTypeConfiguration<GuildDbEntity>
{
    public void Configure(EntityTypeBuilder<GuildDbEntity> builder)
    {
        builder.HasMany(u => u.Incidents)
            .WithOne(i => i.Guild)
            .HasForeignKey(i => i.GuildId)
            .HasPrincipalKey(x => x.DiscordId);

        builder.HasOne(a => a.Settings)
            .WithOne(b => b.Guild)
            .HasForeignKey<GuildDbEntity>(b => b.DiscordId)
            .HasPrincipalKey<GuildConfigDbEntity>(x => x.DiscordGuildId);

        builder.HasMany(x => x.Quotes)
            .WithOne(x => x.Guild)
            .HasForeignKey(x => x.DiscordGuildId)
            .HasPrincipalKey(x => x.DiscordId);
    }
}