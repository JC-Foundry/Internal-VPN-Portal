using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VPN_Portal.Models.Security;

public class CreateUserEvent : ISecurityEvent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(50)]
    public string EventId { get; set; }
    [ForeignKey(nameof(EventId))]
    public virtual SecurityEvent SecurityEvent { get; set; }
    
    public CreatedUserType CreatedBy { get; set; } = CreatedUserType.Unknown;
}

public enum CreatedUserType
{
    Admin,
    Register,
    Unknown
}