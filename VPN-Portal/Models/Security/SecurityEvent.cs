using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VPN_Portal.Authentication;

namespace VPN_Portal.Models.Security;

public class SecurityEvent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(50)]
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    
    [MaxLength(450)]
    public string? UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser? User { get; set; }
    
    [Required]
    [Column(TypeName = "int")]
    public EventType EventType { get; init; }

    [NotMapped]
    public ThreatSeverity BaseSeverity => EventType switch
    {
        EventType.CreateUser => ThreatSeverity.Info,
        EventType.InvalidLoginAttempt => ThreatSeverity.Low,
        EventType.InvalidToken => ThreatSeverity.Low,
        EventType.AddRouterPeer => ThreatSeverity.Low,
        EventType.UnauthorisedVpnPeer => ThreatSeverity.High,
        EventType.UnauthorisedVpnToken => ThreatSeverity.High,
        EventType.PeerAbuse => ThreatSeverity.Medium,
        EventType.RoleElevation => ThreatSeverity.Medium,       //soon
        EventType.ElevationToAdmin => ThreatSeverity.Critical,  //soon
        _ => throw new ArgumentOutOfRangeException()
    };
    
    [Column(TypeName = "int")]
    public ThreatSeverity? ElevatedSeverity { get; set; }
    
    [Required]
    [Column(TypeName = "int")]
    public EventStatus Status { get; set; } = EventStatus.Open;
    
    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;

    public virtual ICollection<SecurityAction> Actions { get; set; }
}

public enum EventType
{
    CreateUser,
    InvalidLoginAttempt,
    InvalidToken,
    AddRouterPeer,
    UnauthorisedVpnPeer,
    UnauthorisedVpnToken,
    PeerAbuse,
    RoleElevation,
    ElevationToAdmin
}

public enum ThreatSeverity
{
    /// <summary>
    /// Informational alert - often can be ignored
    /// </summary>
    Info,
    /// <summary>
    /// Low-security threat. Should be monitored and looked into at the lowest priority.
    /// </summary>
    Low,
    /// <summary>
    /// Medium-security threat. Could be a sign of something more severe.
    /// </summary>
    Medium,
    /// <summary>
    /// High-security threat. Likely a malicious attack.
    /// </summary>
    High,
    /// <summary>
    /// Critical-security threat. Immediate action required, possible account deletion.
    /// </summary>
    Critical
}

public enum EventStatus
{
    /// <summary>
    /// Final state (hidden from main-view)
    /// </summary>
    Closed = -1,   
    /// <summary>
    /// New security alert
    /// </summary>
    Open,          
    /// <summary>
    /// Opened/Viewed or action taken
    /// </summary>
    Acknowledged,   
    /// <summary>
    /// Muted/Ignored
    /// </summary>
    Suppressed,     
    /// <summary>
    /// Manually marked after review of all actions taken
    /// </summary>
    Resolved       
}