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

public class QuoteDbEntity
{
    [Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }

    [Column("discordGuildId")]
    public ulong GuildId { get; set; }

    /// <summary>
    ///     User which was quoted
    /// </summary>
    [Column("quotedUserId")]
    public ulong QuotedUserId { get; set; }

    /// <summary>
    ///     User who crated this quote
    /// </summary>
    [Column("UserId")]
    public ulong UserId { get; set; }

    /// <summary>
    ///     Quoted content
    /// </summary>
    [Column("content"), MaxLength(1000)]
    public string Content { get; set; }

    [Column("timestamp")]
    public DateTime CreatedAt { get; set; }

    public GuildDbEntity Guild { get; set; }
}