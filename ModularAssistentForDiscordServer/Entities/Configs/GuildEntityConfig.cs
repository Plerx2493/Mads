using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MADS.Entities.Configs;

public class GuildEntityConfig  : IEntityTypeConfiguration<GuildDbEntity>
{
    public void Configure(EntityTypeBuilder<GuildDbEntity> builder)
    {
        builder
            .HasOne(g => g.Config)
            .WithOne(g => g.Guild)
            .HasForeignKey<GuildDbEntity>(g => g.Id);

        builder.HasMany(u => u.Incidents)
               .WithOne(i => i.Guild)
               .HasForeignKey(i => i.Id);
    }
}