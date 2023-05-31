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

using System.ComponentModel.DataAnnotations.Schema;

namespace MADS.Entities;

public class ReminderDbEntity
{
    [Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }

    [Column("userId")]
    public ulong UserId { get; set; }

    [Column("reminderText")]
    public string ReminderText { get; set; }

    [Column("creationTime")]
    public DateTime CreationTime { get; set; }

    [Column("executionTime")]
    public DateTime ExecutionTime { get; set; }

    [Column("isPrivate")]
    public bool IsPrivate { get; set; }

    [Column("channelId")]
    public ulong ChannelId { get; set; }

    [Column("messageId")]
    public ulong MessageId { get; set; }

    [Column("mentionedChannel")]
    public ulong MentionedChannel { get; set; }

    [Column("MentionedMessage")]
    public ulong MentionedMessage { get; set; }

    public UserDbEntity User { get; set; }
}