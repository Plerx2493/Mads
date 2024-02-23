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

using MADS.Entities;
using Microsoft.Extensions.Logging;
// ReSharper disable NotResolvedInText

namespace MADS.Extensions;

internal static class DataProvider
{
    public static MadsConfig GetConfig()
    {
        MadsConfig config = new()
        {
            Token = Environment.GetEnvironmentVariable("MADS_DISCORD_TOKEN") ?? throw new ArgumentNullException("Missing env var MADS_DISCORD_TOKEN"),
            Prefix = Environment.GetEnvironmentVariable("MADS_DEFAULT_PREFIX") ?? throw new ArgumentNullException("Missing env var MADS_DEFAULT_PREFIX"),
            LogLevel = Enum.Parse<LogLevel>(Environment.GetEnvironmentVariable("MADS_MINIMUM_LOG_LEVEL") ?? throw new ArgumentNullException("Missing env var MADS_MINIMUM_LOG_LEVEL")),
            ConnectionString = Environment.GetEnvironmentVariable("MADS_DATABASE_CONNECTION_STRING") ?? throw new ArgumentNullException("Missing env var MADS_DATABASE_CONNECTION_STRING"),
            ConnectionStringQuartz = Environment.GetEnvironmentVariable("MADS_DATABASE_CONNECTION_STRING_QUARTZ") ?? throw new ArgumentNullException("Missing env var MADS_DATABASE_CONNECTION_STRING_QUARTZ"),
            DiscordWebhook = Environment.GetEnvironmentVariable("MADS_DISCORD_WEBHOOK") ?? throw new ArgumentNullException("Missing env var MADS_DISCORD_WEBHOOK"),
            DmProxyChannelId = ulong.Parse(Environment.GetEnvironmentVariable("MADS_DM_PROXY_CHANNEL_ID") ?? throw new ArgumentNullException("Missing env var MADS_DM_PROXY_CHANNEL_ID")),
            DeeplApiKey = Environment.GetEnvironmentVariable("MADS_DEEPL_API_KEY") ?? throw new ArgumentNullException("Missing env var MADS_DEEPL_API_KEY")
        };

        return config;
    }
    public static string GetPath(params string[] path)
    {
        return Path.GetFullPath(Path.Combine(path));
    }
}