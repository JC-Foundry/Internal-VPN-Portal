using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VPN_Portal.Models.Devices;
using VPN_Portal.Models.VpnServers;

namespace VPN_Portal.Models.Security;

public class UnauthorisedVpnPeerEvent : ISecurityEvent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(50)]
    public string EventId { get; set; }
    [ForeignKey(nameof(EventId))]
    public virtual SecurityEvent SecurityEvent { get; set; }
    
    [Required]
    public string VpnServerId { get; set; }
    [ForeignKey(nameof(VpnServerId))]
    public virtual VpnServer VpnServer { get; set; }
    
    [Required]
    public string PeerId { get; set; }
    [ForeignKey(nameof(PeerId))]
    public virtual DevicePeer Peer { get; set; }
    
    [Required]
    public DateTime PeerCreatedUtc { get; set; }
}