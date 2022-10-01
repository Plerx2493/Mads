using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MADS.Entities;

[Table("guilds")]
public class GuildDbEntity
{
    [Key]
    [Column("Id")]
    public ulong Id = 0;
    
    [Required]
    [Column("prefix")]
    public string Prefix = "!";

    [Required]
    public GuildConfigDbEntity Config { get; set; }

    public List<GuildUserDbEntity> Users { get; set; }

    public List<IncidentDbEntity> Incidents { get; set; }
}