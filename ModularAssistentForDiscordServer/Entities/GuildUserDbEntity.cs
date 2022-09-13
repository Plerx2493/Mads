using System.ComponentModel.DataAnnotations;

namespace MADS.Entities;

public class GuildUserDbEntity
{
    [Key]
    public ulong UserId { get;  set; }
    public ulong GuildId { get; set; }
    
    public GuildDbEntity Guild { get; set; }
    public UserDbEntity  User  { get; set; }
}