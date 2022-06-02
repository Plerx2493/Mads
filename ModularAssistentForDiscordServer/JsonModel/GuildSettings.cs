using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace MADS.JsonModel
{
    internal struct GuildSettings
    {
        [JsonProperty("prefix")]
        public string Prefix { get; set; }

        [JsonProperty("logLevel")]
        public LogLevel LogLevel { get; set; }

        [JsonProperty("standardEmbed")]
        public DiscordEmbedBuilder DiscordEmbed { get; set; }

        [JsonProperty("aktivModules")]
        public List<string> AktivModules { get; set; }

        [JsonProperty("AuditChannel")]
        public ulong AuditChannel { get; set; }

        [JsonProperty("AuditLogs")]
        public bool AuditLogs { get; set; }
    }
}