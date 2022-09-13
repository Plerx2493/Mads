using System.ComponentModel.DataAnnotations;

namespace MADS.Entities;

public class GuildDbEntity
{
    [Required]
    public string Prefix = "";

    [Key]
    public ulong Id { get; set; }

    [Required]
    public GuildConfigDbEntity Config { get; set; }

    public List<GuildUserDbEntity> Users { get; set; }

    public List<GuildIncidentDbEntity> Incidents { get; set; }
}