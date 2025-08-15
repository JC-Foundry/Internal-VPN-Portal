using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using VPN_Portal.Models.VpnServers;

namespace VPN_Portal.Models.Devices;

[Table("PeerToReservations")]
[PrimaryKey(nameof(PeerId), nameof(ReservationId))]
[Index(nameof(PeerId))]
[Index(nameof(ReservationId))]
public class PeerToReservation
{
    [Required]
    [MaxLength(50)]
    public string PeerId { get; set; }
    
    [ForeignKey(nameof(PeerId))]
    public virtual DevicePeer? Peer { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string ReservationId { get; set; }
    
    [ForeignKey(nameof(ReservationId))]
    public virtual DnsReservation? DnsReservation { get; set; }

    [NotMapped]
    public bool IsActive => DnsReservation?.ReleasedUtc == null;
}