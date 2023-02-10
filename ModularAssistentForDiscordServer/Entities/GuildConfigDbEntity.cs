using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MADS.Entities;

public class GuildConfigDbEntity
{
    public GuildConfigDbEntity()
    {
    }

    public GuildConfigDbEntity(ulong guildId, string prefix)
    {
        DiscordGuildId = guildId;
        Prefix = prefix;
    }

    public GuildConfigDbEntity(GuildConfigDbEntity old)
    {
        DiscordGuildId = old.DiscordGuildId;
        Prefix = old.Prefix;
    }

    public GuildConfigDbEntity(ulong guildId)
    {
        DiscordGuildId = guildId;
        Prefix = "!";
        StarboardActive = false;
    }

    [Key, Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; init; }

    /// <summary>
    ///     Snowflake id of the guild the config is related to
    /// </summary>
    [Required, Column("discordId")]
    public ulong DiscordGuildId { get; set; }

    [Column("prefix")]
    public string Prefix { get; set; }

    [Column("starboardEnabled")]
    public bool StarboardActive { get; set; }

    [Column("starboardChannel")]
    public ulong? StarboardChannelId { get; set; }

    [Column("starboardThreshold")]
    public int? StarboardThreshold { get; set; }

    [Column("starboardEmojiId")]
    public ulong? StarboardEmojiId { get; set; }

    [Column("starboardEmojiName")]
    public string? StarboardEmojiName { get; set; }

    public GuildDbEntity Guild;
}