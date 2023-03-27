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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MADS.Entities;

public class IncidentDbEntity
{
    [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }

    [Column("guild_id")]
    public ulong GuildId { get; set; }

    [Column("target_id")]
    public ulong TargetId { get; set; }

    [Column("moderator_id")]
    public ulong ModeratorId { get; set; }

    [Column("reason")]
    public string Reason { get; set; } = "not given";


    public GuildDbEntity Guild { get; set; }
    public UserDbEntity TargetUser { get; set; }
    public DateTimeOffset CreationTimeStamp { get; set; }
}