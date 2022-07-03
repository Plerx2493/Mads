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
            string[] parameter = new string[path.Length+1];

            parameter[0] = Path.GetFullPath(Directory.GetCurrentDirectory());

            for (int i = 1; i < path.Length; i++)
            {
                parameter[i] = path[i];
            }

            return Path.GetFullPath(Path.Combine(path));
        }
    }
}

