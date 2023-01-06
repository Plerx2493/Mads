using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MADS.Entities;

public class GuildConfigDbEntity
{
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

    public GuildConfigDbEntity()
    {
        DiscordGuildId = 0;
        Prefix = "!";
    }

    [Key, Column("id"), DefaultValue(0), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; init; }

    /// <summary>
    ///     Snowflake id of the guild the config is related to
    /// </summary>
    [Required, Column("discordId")]
    public ulong DiscordGuildId { get; set; }

    [Column("prefix")]
    public string Prefix { get; set; }
}