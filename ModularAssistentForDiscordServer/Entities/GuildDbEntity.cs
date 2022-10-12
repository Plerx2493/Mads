using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MADS.Entities;

public class GuildDbEntity
{
    [Key]
    [Column("id")]
    [DefaultValue(0)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; init; }
    
    [Required]
    [Column("discordId")]
    public ulong DiscordId { get; set; }   

    [Required]
    [Column("prefix")]
    public string Prefix { get; set; }

    [Required]
    public GuildConfigDbEntity Config { get; set; }

    public List<GuildUserDbEntity> Users { get; set; }

    public List<IncidentDbEntity> Incidents { get; set; }

    public GuildDbEntity()
    {
        Id = 0;
        Prefix = "!";
        Config = new GuildConfigDbEntity();
        Users = new List<GuildUserDbEntity>();
        Incidents = new List<IncidentDbEntity>();
    }

    public GuildDbEntity(GuildDbEntity old)
    {
        Id = old.Id;
        Prefix = old.Prefix;
        Config = old.Config;
        Users = old.Users;
        Incidents = old.Incidents;
    }
}