using Newtonsoft.Json;

namespace MADS.Extensions
{
    internal class JsonProvider
    {
        public static JsonModel readFile<JsonModel>(string path)
        {
            using StreamReader streamReader = File.OpenText(path);
            return JsonConvert.DeserializeObject<JsonModel>(streamReader.ReadToEnd());
        }

        public static JsonModel[] readFileToArray<JsonModel>(string path)
        {
            using StreamReader streamReader = File.OpenText(path);
            return JsonConvert.DeserializeObject<JsonModel[]>(streamReader.ReadToEnd());
        }

        public static List<JsonModel> readFileToList<JsonModel>(string path)
        {
            using StreamReader streamReader = File.OpenText(path);
            return JsonConvert.DeserializeObject<List<JsonModel>>(streamReader.ReadToEnd());
        }

        public static void parseJsonArray<JsonModel>(string path, JsonModel[] model)
        {
            string parsedJson = JsonConvert.SerializeObject(model, Formatting.Indented);
            File.WriteAllText(path, parsedJson);
        }

        public static void parseJson<JsonModel>(string path, JsonModel model)
        {
            string parsedJson = JsonConvert.SerializeObject(model, Formatting.Indented);
            File.WriteAllText(path, parsedJson);
        }
    }
}
