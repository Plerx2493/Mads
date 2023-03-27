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

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MADS.Entities;

public class StarboardMessageDbEntity
{
    [Key, Column("id"), DefaultValue(0), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; init; }

    [Required, Column("discordMessageId")]
    public ulong DiscordMessageId { get; set; }
    
    [Required, Column("discordChannelId")]
    public ulong DiscordChannelId { get; set; }
    
    [Required, Column("discordGuildId")]
    public ulong DiscordGuildId { get; set; }
    
    [Required, Column("starCount")]
    public int Stars { get; set; }
    
    [Required, Column("starboardMessageId"), DefaultValue(0)]
    public ulong StarboardMessageId { get; set; }
    
    [Required, Column("starboardChannelId"), DefaultValue(0)]
    public ulong StarboardChannelId { get; set; }
    
    [Required, Column("starboardGuildId"), DefaultValue(0)]
    public ulong StarboardGuildId { get; set; }
}