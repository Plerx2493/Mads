using Microsoft.Extensions.Logging;

namespace MADS.Entities;

public struct GuildSettings
{
    public GuildSettings()
    {
        Prefix = "!";
        LogLevel = LogLevel.Information;
        AuditChannel = 0;
    }

    public string   Prefix   { get; set; }
    public LogLevel LogLevel { get; set; }
    public ulong? AuditChannel { get; set; }
}