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

using Microsoft.Extensions.Logging;

namespace MADS.Entities;

public class GuildSettings
{
    public GuildSettings()
    {
        Prefix = "!";
        LogLevel = LogLevel.Information;
        AuditChannel = 0;
    }

    public string   Prefix       { get; set; }
    public LogLevel LogLevel     { get; set; }
    public ulong?   AuditChannel { get; set; }

    public ulong    GuildId      { get; set; }
}