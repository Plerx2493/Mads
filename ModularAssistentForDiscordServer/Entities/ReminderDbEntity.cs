using System.ComponentModel.DataAnnotations.Schema;

namespace MADS.Entities;

public class ReminderDbEntity
{
    [Column("id"), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }
    
    [Column("userId")]
    public ulong UserId { get; set; }
    
    [Column("reminderText")]
    public string ReminderText { get; set; }
    
    [Column("creationTime")]
    public DateTime CreationTime { get; set; }
    
    [Column("executionTime")]
    public DateTime ExecutionTime { get; set; }
    
    [Column("isPrivate")]
    public bool IsPrivate { get; set; }
    
    [Column("channelId")]
    public ulong ChannelId { get; set; }
    
    [Column("messageId")]
    public ulong MessageId { get; set; }
    
    [Column("mentionedChannel")]
    public ulong MentionedChannel { get; set; }
    
    [Column("MentionedMessage")]
    public ulong MentionedMessage { get; set; }
    
    public UserDbEntity User { get; set; }
}