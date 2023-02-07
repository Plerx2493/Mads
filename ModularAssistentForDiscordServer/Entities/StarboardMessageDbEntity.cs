using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MADS.Entities;

public class StarboardMessageDbEntity
{
    [Key, Column("id"), DefaultValue(0), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; init; }

    [Required, Column("discordMessageId")]
    public ulong DiscordMessageId { get; set; }
    
    [Required, Column("discordChannelId")]
    public ulong DiscordChannelId { get; set; }
    
    [Required, Column("discordGuildId")]
    public ulong DiscordGuildId { get; set; }
    
    [Required, Column("starCount")]
    public int Stars { get; set; }
    
    [Required, Column("starboardMessageId"), DefaultValue(0)]
    public ulong StarboardMessageId { get; set; }
    
    [Required, Column("starboardChannelId"), DefaultValue(0)]
    public ulong StarboardChannelId { get; set; }
    
    [Required, Column("starboardGuildId"), DefaultValue(0)]
    public ulong StarboardGuildId { get; set; }
}