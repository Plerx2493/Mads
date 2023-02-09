using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MADS.Entities.Configs;

public class GuildConfigConfig : IEntityTypeConfiguration<GuildConfigDbEntity>
{
    public void Configure(EntityTypeBuilder<GuildConfigDbEntity> builder)
    {
        builder.HasOne(x => x.Guild)
            .WithOne(x => x.Settings);
    }
}