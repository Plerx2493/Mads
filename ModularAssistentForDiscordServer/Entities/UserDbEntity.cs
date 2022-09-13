using System.ComponentModel.DataAnnotations;

namespace MADS.Entities;

public class UserDbEntity
{
    [Key]
    public ulong ID { get; set; }
}