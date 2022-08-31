using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MADS.JsonModel
{
    internal struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("defaultPrefix")]
        public string Prefix { get; set; }

        [JsonProperty("minmumloglvl")]
        public LogLevel LogLevel { get; set; }

        [JsonProperty("defaultEmbed")]
        public DiscordEmbedBuilder DiscordEmbed { get; set; }

        [JsonProperty("guildSettings")]
        public Dictionary<ulong, GuildSettings> GuildSettings { get; set; }
    }
}