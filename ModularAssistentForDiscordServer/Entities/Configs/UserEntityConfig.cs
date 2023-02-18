using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MADS.Entities.Configs;

public class UserEntityConfig : IEntityTypeConfiguration<UserDbEntity>
{
    public void Configure(EntityTypeBuilder<UserDbEntity> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.HasMany(u => u.Reminders)
               .WithOne(x => x.User)
               .HasPrincipalKey(x => x.Id)
               .HasForeignKey(x => x.UserId);
        
        builder.HasMany(u => u.Incidents)
               .WithOne(x => x.TargetUser)
               .HasPrincipalKey(x => x.Id)
               .HasForeignKey(x => x.TargetId);
    }
}