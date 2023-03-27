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

namespace MADS.JsonModel;

public class OAuthTokenJson
{
    [JsonProperty("userTokenDict")]
    public Dictionary<ulong, string> Token { get; set; }

    [JsonProperty("TokenCount")]
    public int TokenCount { get; set; }

    [JsonProperty("ApplicationId")]
    public ulong ApplicationId { get; set; }
}