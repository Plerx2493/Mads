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

using Microsoft.Extensions.Logging;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace MADS.Entities;

public class MadsConfig
{
    public string Token { get; set; }
    
    public string Prefix { get; set; }
    
    public LogLevel LogLevel { get; set; }
    
    public string ConnectionString { get; set; }

    public string ConnectionStringQuartz { get; set; }

    public string DiscordWebhook { get; set; }

    public ulong? DmProxyChannelId { get; set; }

    public string? DeeplApiKey { get; set; }
}