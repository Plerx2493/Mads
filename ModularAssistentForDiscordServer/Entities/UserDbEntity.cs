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

public class UserDbEntity
{
    [Key, Column("id")]
    public ulong Id { get; set; }

    [Column("username")]
    public string Username { get; set; }

    [Column("discriminator")]
    public int Discriminator { get; set; }
    
    [Column("prefered_language")]
    public string PreferedLanguage { get; set; }

    public List<IncidentDbEntity> Incidents { get; set; }

    public List<ReminderDbEntity> Reminders { get; set; }
    
    public List<VoiceAlert> VoiceAlerts { get; set; }
}