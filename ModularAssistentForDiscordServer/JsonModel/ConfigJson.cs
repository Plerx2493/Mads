using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MADS.JsonModel;

public struct ConfigJson
{
    [JsonProperty("token")]
    public string Token { get; set; }

    [JsonProperty("defaultPrefix")]
    public string Prefix { get; set; }

    [JsonProperty("minmumloglvl")]
    public LogLevel LogLevel { get; set; }

    //[JsonProperty("databaseConnectionString")]
    //public string ConnectionString { get; set; }

    [JsonProperty("discordWebhook")]
    public string DiscordWebhook { get; set; }

    [JsonProperty("DmProxyChannelId")]
    public ulong? DmProxyChannelId { get; set; }
}