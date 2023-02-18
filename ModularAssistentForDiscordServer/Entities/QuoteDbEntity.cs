using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MADS.Entities;

public class QuoteDbEntity
{
    [Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }

    [Column("discordGuildId")]
    public ulong DiscordGuildId { get; set; }

    /// <summary>
    /// User which was quoted
    /// </summary>
    [Column("quotedUserId")]
    public ulong QuotedUserId { get; set; }

    /// <summary>
    /// User who crated this quote
    /// </summary>
    [Column("UserId")]
    public ulong UserId { get; set; }

    /// <summary>
    /// Quoted content
    /// </summary>
    [Column("content"), MaxLength(1000)]
    public string Content { get; set; }

    [Column("timestamp")]
    public DateTime CreatedAt { get; set; }

    public GuildDbEntity Guild { get; set; }
}