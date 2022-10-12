using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MADS.Entities;

public class GuildUserDbEntity
{
    [Key]
    [Column("id")]
    [DefaultValue(0)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; init; }
    
    [Required]
    [Column("discordId")]
    public ulong DiscordId { get; set; }

    [Column("guildId")]
    public ulong GuildId { get; set; }

    public GuildDbEntity Guild { get; set; }
    public UserDbEntity User { get; set; }
}