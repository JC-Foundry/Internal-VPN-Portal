using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using VPN_Portal.Models.VpnServers;

namespace VPN_Portal.Models.Devices;

[Table("DevicePeers")]
[Index(nameof(DeviceId))]
public class DevicePeer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(50)]
    public string PeerId { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(50)]
    public string DeviceId { get; set; }
    [ForeignKey(nameof(DeviceId))]
    public virtual Device? Device { get; set; }
    
    [Required, MaxLength(50)]
    public string VpnServerId { get; set; } 
    [ForeignKey(nameof(VpnServerId))]
    public virtual VpnServer? VpnServer { get; set; }
    
    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    
    public bool IsDeleted { get; set; }
    
    [Column(TypeName = "datetime2")]
    public DateTime? DeletedUtc { get; set; }

    public string? AssignedIp()
    {
        var activeReservation = PeerToReservations?.FirstOrDefault(ptr => ptr.IsActive);
        if(activeReservation?.DnsReservation?.DnsPool == null) return null;
        
        var network = activeReservation.DnsReservation.DnsPool.Family switch
        {
            NetworkFamily.Net10 => "10.",
            NetworkFamily.Net172 => "172.",
            NetworkFamily.Net192 => "192.168",
            _ => null
        };
        
        if(network == null) return null;

        var ip = network;
        if (activeReservation.DnsReservation.DnsPool.Family != NetworkFamily.Net192)
            ip += $"{activeReservation.DnsReservation.DnsPool.SecondOctet}.";

        return $"{ip}.{activeReservation.DnsReservation.DnsPool.Subnet}.{activeReservation.DnsReservation.HostOctet}";
    }
    
    public virtual ICollection<PeerToReservation> PeerToReservations { get; set; }
}