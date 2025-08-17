using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VPN_Portal.Models.VpnServers;

[Table("DnsPools")]
[Index(nameof(Name), IsUnique = true)]
[Index(nameof(Enabled))]
public class DnsPool
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(50)]
    public string DnsPoolId { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [Column(TypeName = "int")]
    public NetworkFamily Family { get; set; } = NetworkFamily.Net192;
    
    [Column(TypeName = "int")]
    public ushort? SecondOctet { get; private set; }
    
    [Required]
    [Range(0, 255)]
    [Column(TypeName = "int")]
    public ushort Subnet { get; set; }
    
    [Required]
    [Range(2, 254)]
    [Column(TypeName = "int")]
    public ushort MinHost { get; set; }
    
    [Required]
    [Range(2, 254)]
    [Column(TypeName = "int")]
    public ushort MaxHost { get; set; }
    
    [Required]
    public bool Enabled { get; set; } = true;
    
    public virtual ICollection<DnsReservation>? Reservations { get; set; }
    public virtual ICollection<VpnServer>? VpnServers { get; set; }

    public bool SetSecondOctet(ushort value)
    {
        switch (Family)
        {
            case NetworkFamily.Net10:
                if(value > 255) return false;
                SecondOctet = value;
                return true;
            case NetworkFamily.Net172:
                if(value is < 16 or > 31) return false;
                SecondOctet = value;
                return true;
            default:
                SecondOctet = null;
                return false;
        }
    }

    public string GetRange()
    {
        switch (Family)
        {
            case NetworkFamily.Net10:
                return $"10.{SecondOctet}.{Subnet}.{MinHost}-{MaxHost}";
            case NetworkFamily.Net172:
                return $"172.{SecondOctet}.{Subnet}.{MinHost}-{MaxHost}";
            default:
                return $"192.168.{Subnet}.{MinHost}-{MaxHost}";
        }
    }
}

public enum NetworkFamily
{
    Net10,      //10.x.y.z
    Net172,     //172.16-31.y.z
    Net192,     //192.168.y.z
}