using MADS.JsonModel;
using Microsoft.CodeAnalysis;

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