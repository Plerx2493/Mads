using Microsoft.Extensions.Logging;

namespace MADS.Entities;

public class GuildSettings
{
    public GuildSettings()
    {
        Prefix = "!";
        LogLevel = LogLevel.Information;
        AuditChannel = 0;
    }

    public string   Prefix       { get; set; }
    public LogLevel LogLevel     { get; set; }
    public ulong?   AuditChannel { get; set; }

    public ulong    GuildId      { get; set; }
}