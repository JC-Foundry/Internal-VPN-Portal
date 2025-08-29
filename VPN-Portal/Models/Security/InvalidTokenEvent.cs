using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VPN_Portal.Models.Devices;

namespace VPN_Portal.Models.Security;

public class InvalidTokenEvent : ISecurityEvent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(50)]
    public string EventId { get; set; }
    [ForeignKey(nameof(EventId))]
    public virtual SecurityEvent SecurityEvent { get; set; }
    
    public string? TokenId { get; set; }
    [ForeignKey(nameof(TokenId))]
    public virtual DownloadToken? Token { get; set; }
    
    public string? PeerId { get; set; }
    [ForeignKey(nameof(PeerId))]
    public virtual DevicePeer? Peer { get; set; }
    
    [Required]
    [Column(TypeName = "int")]
    public DownloadOutcome Outcome { get; set; }
    
    [Required]
    [Column(TypeName = "int")]
    public DownloadTokenPurpose Purpose { get; set; }
}