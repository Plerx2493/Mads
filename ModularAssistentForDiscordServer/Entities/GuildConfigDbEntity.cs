using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MADS.Entities;

public class GuildConfigDbEntity
{
    [Key]
    [Column("guild_id")]
    public ulong GuildId { get;set; }
    
    [Column("prefix")]
    public string Prefix { get; set; }
    
    
    public GuildDbEntity Guild { get; set; }

    public GuildConfigDbEntity(ulong guildId, string prefix, GuildDbEntity guild)
    {
        GuildId = guildId;
        Prefix = prefix;
        Guild = guild;
    }

    public GuildConfigDbEntity(GuildConfigDbEntity old)
    {
        GuildId = old.GuildId;
        Prefix = old.Prefix;
        Guild = old.Guild;
    }

    public GuildConfigDbEntity()
    {
    }
}           