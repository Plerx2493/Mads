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

#nullable enable
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MADS.Entities;

public class GuildDbEntity
{
    public GuildDbEntity()
    {
        Id = 0;
        Incidents = new List<IncidentDbEntity>();
        Settings = new GuildConfigDbEntity();
        Quotes = new List<QuoteDbEntity>();
    }

    public GuildDbEntity(GuildDbEntity old)
    {
        Id = old.Id;
        Incidents = old.Incidents;
        Settings = old.Settings;
        Quotes = old.Quotes;
    }

    [Key, Column("id"), DefaultValue(0), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; init; }

    [Required, Column("discordId")]
    public ulong DiscordId { get; set; }

    public GuildConfigDbEntity Settings { get; set; }

    public List<IncidentDbEntity> Incidents { get; set; }

    public List<QuoteDbEntity> Quotes { get; set; }
}