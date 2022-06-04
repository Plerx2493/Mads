using MADS.JsonModel;
using Newtonsoft.Json;

namespace MADS.Extensions
{
    internal class DataProvider
    {
        public DataProvider()
        {
        }

        public static ConfigJson GetConfig()
        {
            return JsonProvider.readFile<ConfigJson>(GetPath("config.json"));
        }

        public static JsonModel GetJson<JsonModel>(string path)
        {
            return JsonProvider.readFile<JsonModel>(GetPath(path));
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

