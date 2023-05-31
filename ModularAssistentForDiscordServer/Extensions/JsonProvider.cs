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

using Newtonsoft.Json;

namespace MADS.Services;

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