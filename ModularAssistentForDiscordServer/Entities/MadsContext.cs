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

namespace MADS.Entities;

public class MadsContext : DbContext
{
    public MadsContext(DbContextOptions<MadsContext> options) : base(options)
    {
    }

    public DbSet<UserDbEntity>             Users     { get; set; }
    public DbSet<GuildDbEntity>            Guilds    { get; set; }
    public DbSet<GuildConfigDbEntity>      Configs   { get; set; }
    public DbSet<IncidentDbEntity>         Incidents { get; set; }
    public DbSet<StarboardMessageDbEntity> Starboard { get; set; }
    public DbSet<QuoteDbEntity>            Quotes    { get; set; }
    public DbSet<ReminderDbEntity>         Reminders { get; set; } 

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(MadsContext).Assembly);

        base.OnModelCreating(builder);
    }
}