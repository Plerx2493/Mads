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
}           