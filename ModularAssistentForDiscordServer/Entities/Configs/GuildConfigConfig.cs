using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace MADS.Entities.Configs;

public class GuildConfigConfig : IEntityTypeConfiguration<GuildConfigDbEntity>
{
    public void Configure(EntityTypeBuilder<GuildConfigDbEntity> builder)
    {
        
    } 
}