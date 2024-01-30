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

using System.Configuration;
using MADS.JsonModel;
using Microsoft.Extensions.Logging;

namespace MADS.Extensions;

internal static class DataProvider
{
    public static MadsConfig GetConfig()
    {
        var config = new MadsConfig();
        
        config.Token = Environment.GetEnvironmentVariable("MADS_DISCORD_TOKEN") ?? throw new ArgumentNullException("Missing env var MADS_DISCORD_TOKEN");
        config.Prefix = Environment.GetEnvironmentVariable("MADS_DEFAULT_PREFIX") ?? throw new ArgumentNullException("Missing env var MADS_DEFAULT_PREFIX");
        config.LogLevel = Enum.Parse<LogLevel>(Environment.GetEnvironmentVariable("MADS_MINIMUM_LOG_LEVEL") ?? throw new ArgumentNullException("Missing env var MADS_MINIMUM_LOG_LEVEL"));
        config.ConnectionString = Environment.GetEnvironmentVariable("MADS_DATABASE_CONNECTION_STRING") ?? throw new ArgumentNullException("Missing env var MADS_DATABASE_CONNECTION_STRING");
        config.ConnectionStringQuartz = Environment.GetEnvironmentVariable("MADS_DATABASE_CONNECTION_STRING_QUARTZ") ?? throw new ArgumentNullException("Missing env var MADS_DATABASE_CONNECTION_STRING_QUARTZ");
        config.DiscordWebhook = Environment.GetEnvironmentVariable("MADS_DISCORD_WEBHOOK") ?? throw new ArgumentNullException("Missing env var MADS_DISCORD_WEBHOOK");
        config.DmProxyChannelId = ulong.Parse(Environment.GetEnvironmentVariable("MADS_DM_PROXY_CHANNEL_ID") ?? throw new ArgumentNullException("Missing env var MADS_DM_PROXY_CHANNEL_ID"));
        config.DeeplApiKey = Environment.GetEnvironmentVariable("MADS_DEEPL_API_KEY") ?? throw new ArgumentNullException("Missing env var MADS_DEEPL_API_KEY");
        
        return config;
    }

    public static TJsonModel GetJson<TJsonModel>(string path)
    {
        return JsonProvider.ReadFile<TJsonModel>(GetPath(path));
    }

    public static void SetConfig(MadsConfig configJson)
    {
        JsonProvider.ParseJson(GetPath("config.json"), configJson);
    }

    public static string GetPath(params string[] path)
    {
        return Path.GetFullPath(Path.Combine(path));
    }

    public static bool TryGetOAuthTokenByUser(ulong userId, out string token)
    {
        var file = JsonProvider.ReadFile<OAuthTokenJson>(GetPath("oauthtoken.json"));

        return file.Token.TryGetValue(userId, out token);
    }

    public static bool TryInsertUserToken(ulong userId, string token)
    {
        var file = JsonProvider.ReadFile<OAuthTokenJson>(GetPath("oauthtoken.json"));

        var success = file.Token.TryAdd(userId, token);

        file.TokenCount++;

        JsonProvider.ParseJson(GetPath("config.json"), file);

        return success;
    }
}