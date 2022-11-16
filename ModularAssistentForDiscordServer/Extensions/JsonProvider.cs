using Newtonsoft.Json;

namespace MADS.Extensions;

internal static class JsonProvider
{
    public static TJsonModel ReadFile<TJsonModel>(string path)
    {
        using var streamReader = File.OpenText(path);
        return JsonConvert.DeserializeObject<TJsonModel>(streamReader.ReadToEnd());
    }

    public static TJsonModel[] ReadFileToArray<TJsonModel>(string path)
    {
        using var streamReader = File.OpenText(path);
        return JsonConvert.DeserializeObject<TJsonModel[]>(streamReader.ReadToEnd());
    }

    public static List<TJsonModel> ReadFileToList<TJsonModel>(string path)
    {
        using var streamReader = File.OpenText(path);
        return JsonConvert.DeserializeObject<List<TJsonModel>>(streamReader.ReadToEnd());
    }

    public static void ParseJsonArray<TJsonModel>(string path, TJsonModel[] model)
    {
        var parsedJson = JsonConvert.SerializeObject(model, Formatting.Indented);
        File.WriteAllText(path, parsedJson);
    }

    public static void ParseJson<TJsonModel>(string path, TJsonModel model)
    {
        var parsedJson = JsonConvert.SerializeObject(model, Formatting.Indented);
        File.WriteAllText(path, parsedJson);
    }
}