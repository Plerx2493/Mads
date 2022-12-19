using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MADS.Entities;

public class UserDbEntity
{
    [Key, Column("id")]
    public ulong Id { get; set; }

    public List<GuildUserDbEntity> Guilds    { get; set; }
    public List<IncidentDbEntity>  Incidents { get; set; }
}