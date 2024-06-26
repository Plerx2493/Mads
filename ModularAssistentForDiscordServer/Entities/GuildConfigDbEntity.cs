﻿// Copyright 2023 Plerx2493
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

public class GuildConfigDbEntity
{
    [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; init; }

    /// <summary>
    /// Snowflake id of the guild the config is related to
    /// </summary>
    [Required, Column("discordId")]
    public ulong GuildId { get; set; }

    [Column("prefix"), MaxLength(5)]
    public string Prefix { get; set; }

    [Column("starboardEnabled")]
    public bool StarboardActive { get; set; }

    [Column("starboardChannel")]
    public ulong? StarboardChannelId { get; set; }

    [Column("starboardThreshold")]
    public int? StarboardThreshold { get; set; }

    [Column("starboardEmojiId")]
    public ulong? StarboardEmojiId { get; set; }

    [Column("starboardEmojiName"), MaxLength(50)]
    public string? StarboardEmojiName { get; set; }
    
    public GuildDbEntity Guild;
}