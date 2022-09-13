using MADS.Entities;
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

    public static void SetConfig(Dictionary<ulong, GuildSettings> guildSettings)
    {
        var configJson = GetConfig();

        configJson.GuildSettings = guildSettings;

        JsonProvider.ParseJson(GetPath("config.json"), configJson);
    }

    public static void SetConfig(ConfigJson configJson)
    {
        JsonProvider.ParseJson(GetPath("config.json"), configJson);
    }

    public static string GetPath(params string[] path)
    {
        return Path.GetFullPath(Path.Combine(path));
    }
}