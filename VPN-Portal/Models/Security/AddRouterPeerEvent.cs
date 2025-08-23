using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VPN_Portal.Models.Devices;

namespace VPN_Portal.Models.Security;

public class AddRouterPeerEvent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(50)]
    public string EventId { get; set; }
    [ForeignKey(nameof(EventId))]
    public virtual SecurityEvent SecurityEvent { get; set; }
    
    public string PeerId { get; set; }
    [ForeignKey(nameof(PeerId))]
    public virtual DevicePeer Peer { get; set; }
    
    [Required]
    public string PublicKey { get; set; }
    
    [Required]
    public string IpAssigned { get; set; }
}