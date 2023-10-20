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

public class VoiceAlert
{
    [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong AlertId { get; set; }
    
    [Column("channel_id")]
    public ulong ChannelId { get; set; }
    
    [Column("guild_id")]
    public ulong GuildId { get; set; }

    [Column("user_id")]
    public ulong UserId { get; set; }
    
    [Column("is_repeatable")]
    public bool IsRepeatable { get; set; }
    
    [Column("last_alert")]
    public DateTimeOffset? LastAlert { get; set; }
    
    [Column("time_between")]
    public TimeSpan? MinTimeBetweenAlerts { get; set; }
    
    public UserDbEntity User { get; set; }
}