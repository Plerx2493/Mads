using Microsoft.Extensions.Logging;

namespace MADS.Entities;

public struct GuildSettings
{
    public GuildSettings()
    {
        Prefix = "!";
        LogLevel = LogLevel.Information;
        AktivModules = new List<string>();
        AuditChannel = 0;
        AuditLogs = false;
    }
    
    public string Prefix { get; set; }
    public LogLevel LogLevel { get; set; }
    public List<string> AktivModules { get; set; }
    public ulong AuditChannel { get; set; }
    public bool AuditLogs { get; set; }
}