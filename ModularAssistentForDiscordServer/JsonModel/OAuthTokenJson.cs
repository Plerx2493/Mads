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