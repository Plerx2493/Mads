using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MADS.Entities;

public class UserDbEntity
{
    [Key, Column("id")]
    public ulong Id { get; set; }

    [Column("username")]
    public string Username { get; set; }

    [Column("discriminator")]
    public int Discriminator { get; set; }

    public List<IncidentDbEntity> Incidents { get; set; }
}