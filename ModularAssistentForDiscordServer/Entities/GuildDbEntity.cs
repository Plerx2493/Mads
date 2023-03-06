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
        Incidents = new List<IncidentDbEntity>();
        Settings = new GuildConfigDbEntity();
        Quotes = new List<QuoteDbEntity>();
    }

    public GuildDbEntity(GuildDbEntity old)
    {
        Id = old.Id;
        Incidents = old.Incidents;
        Settings = old.Settings;
        Quotes = old.Quotes;
    }

    [Key, Column("id"), DefaultValue(0), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; init; }

    [Required, Column("discordId")]
    public ulong DiscordId { get; set; }

    public GuildConfigDbEntity Settings { get; set; }

    public List<IncidentDbEntity> Incidents { get; set; }

    public List<QuoteDbEntity> Quotes { get; set; }
}