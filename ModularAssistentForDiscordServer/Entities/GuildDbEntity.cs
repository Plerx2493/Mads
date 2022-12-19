#nullable enable
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MADS.Entities;

public class GuildDbEntity
{
    public GuildDbEntity()
    {
        Id = 0;
        Users = new List<GuildUserDbEntity>();
        Incidents = new List<IncidentDbEntity>();
    }

    public GuildDbEntity(GuildDbEntity old)
    {
        Id = old.Id;
        Users = old.Users;
        Incidents = old.Incidents;
    }

    [Key, Column("id"), DefaultValue(0), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; init; }

    [Required, Column("discordId")]
    public ulong DiscordId { get; set; }

    public GuildSettings Settings { get; set; }

    public List<GuildUserDbEntity>? Users { get; set; }

    public List<IncidentDbEntity>? Incidents { get; set; }
}