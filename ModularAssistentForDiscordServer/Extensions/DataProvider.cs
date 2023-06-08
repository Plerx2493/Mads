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

using MADS.JsonModel;

namespace MADS.Extensions;

internal static class DataProvider
{
    public static ConfigJson GetConfig()
    {
        return JsonProvider.ReadFile<ConfigJson>(GetPath("config.json"));
    }

    public static TJsonModel GetJson<TJsonModel>(string path)
    {
        return JsonProvider.ReadFile<TJsonModel>(GetPath(path));
    }

    public static void SetConfig(ConfigJson configJson)
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