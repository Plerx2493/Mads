using MADS.JsonModel;

namespace MADS.Extensions
{
    internal static class DataProvider
    {
        public static ConfigJson GetConfig()
        {
            return JsonProvider.readFile<ConfigJson>(GetPath("config.json"));
        }

        public static JsonModel GetJson<JsonModel>(string path)
        {
            return JsonProvider.readFile<JsonModel>(GetPath(path));
        }

        public static void SetConfig(Dictionary<ulong, GuildSettings> guildSettings)
        {
            ConfigJson configJson = GetConfig();

            configJson.GuildSettings = guildSettings;

            JsonProvider.parseJson(GetPath("config.json"), configJson);
        }

        public static void SetConfig(ConfigJson configJson)
        {
            JsonProvider.parseJson(GetPath("config.json"), configJson);
        }

        public static string GetPath(params string[] path)
        {
            return Path.GetFullPath(Path.Combine(path));
        }
    }
}