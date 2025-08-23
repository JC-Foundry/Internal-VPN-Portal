using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VPN_Portal.Authentication;

namespace VPN_Portal.Models.Security;

public class SecurityAction
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string ActionId { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string EventId { get; set; }
    [ForeignKey(nameof(EventId))]
    public virtual SecurityEvent SecurityEvent { get; set; }
    
    public string? UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser? User { get; set; }
    
    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    
    [Required]
    [Column(TypeName = "int")]
    public ActionType ActionType { get; set; }
    
    [Required]
    [Column(TypeName = "int")]
    public TakenByType TakenByType { get; set; }

    public bool IsReversible { get; set; } = true;
}

public enum ActionType
{
    AccountDisabled,
    AccountDeleted,
    AccountEnabled,
    RouterPeerRemoved,
    PeerSoftRemoved,
    PeerRestored,
    PeerHardRemoved,
    RoleRemoved
}

public enum TakenByType
{
    Administrator,
    User,
    System
}