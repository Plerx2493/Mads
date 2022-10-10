﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MADS.Entities.Configs;

public class UserEntityConfig : IEntityTypeConfiguration<UserDbEntity>
{
    public void Configure(EntityTypeBuilder<UserDbEntity> builder)
    {
        builder.HasKey(u => u.Id);
        
        builder.HasMany(u => u.Guilds)
               .WithOne(u => u.User)
               .HasForeignKey(u => u.UserId);
    }
}