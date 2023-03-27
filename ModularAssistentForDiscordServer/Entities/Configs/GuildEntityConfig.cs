// Copyright 2023 Plerx2493
//
// Licensed under the Apache License, Version 2.0 (the "License")
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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