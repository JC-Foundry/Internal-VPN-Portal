using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using VPN_Portal.Models.Devices;

namespace VPN_Portal.Models.VpnServers;

[Table("DnsReservations")]
[Index(nameof(DnsPoolId), nameof(HostOctet), nameof(ReleasedUtc), IsUnique = true)]
[Index(nameof(DeviceId))]
[Index(nameof(ReleasedUtc))]
public class DnsReservation
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(50)]
    public string ReservationId { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(50)]
    public string DnsPoolId { get; set; }
    
    [ForeignKey(nameof(DnsPoolId))]
    public virtual DnsPool DnsPool { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string DeviceId { get; set; }
    
    [ForeignKey(nameof(DeviceId))]
    public virtual Device Device { get; set; }
    
    [Required]
    [Range(2, 254)]
    [Column(TypeName = "int")]
    public ushort HostOctet { get; set; }
    
    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime ReservedUtc { get; set; } = DateTime.UtcNow;
    
    [Column(TypeName = "datetime2")]
    public DateTime? ReleasedUtc { get; set; }
    public bool IsActive => ReleasedUtc == null;

    public string GetAddress()
    {
        if(DnsPool == null!) return "Unknown";
        
        var family = DnsPool.Family;
        var ip = family switch
        {
            NetworkFamily.Net10 => $"10.{DnsPool.SecondOctet}.{DnsPool.Subnet}.{HostOctet}",
            NetworkFamily.Net172 => $"172.{DnsPool.SecondOctet}.{DnsPool.Subnet}.{HostOctet}",
            NetworkFamily.Net192 => $"192.168.{DnsPool.Subnet}.{HostOctet}",
            _ => null
        };
        
        return ip ?? "Unknown";
    }
    
    public virtual ICollection<PeerToReservation> PeerToReservations { get; set; }
}