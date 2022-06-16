﻿using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace MADS.JsonModel
{
    public struct GuildSettings
    {
        public GuildSettings()
        {
            Prefix = "!";
            LogLevel = LogLevel.Information;
            DiscordEmbed = new DiscordEmbedBuilder()
            {
                Color = new(new(0, 255, 194)),
                Footer = new()
                {
                    Text = "Mads"
                }
            };
            AktivModules = new();
            AuditChannel = 0;
            AuditLogs = false;
        }

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