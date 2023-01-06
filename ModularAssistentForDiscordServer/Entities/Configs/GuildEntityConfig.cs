using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MADS.Entities.Configs;

public class GuildEntityConfig : IEntityTypeConfiguration<GuildDbEntity>
{
    public void Configure(EntityTypeBuilder<GuildDbEntity> builder)
    {
        builder.HasMany(u => u.Incidents)
               .WithOne(i => i.Guild)
               .HasForeignKey(i => i.Id);
        builder.HasOne(x => x.Settings);
        builder.HasOne(a => a.Settings)
               .WithOne(b => b.Guild)
               .HasForeignKey<GuildDbEntity>(b => b.Id);
    }
}