using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VPN_Portal.Models.VpnServers;

[Table("VpnServers")]
[Index(nameof(Name), IsUnique = true)]
public class VpnServer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(50)]
    public string VpnServerId { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(50)]
    public string DnsPoolId { get; set; }
    
    [ForeignKey(nameof(DnsPoolId))]
    public virtual DnsPool? DnsPool { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    
    [Required]
    [Range(1, 65535)]
    public uint EndpointPort { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string EndpointHost { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string PublicKey { get; set; }
    
    [Required]
    [MaxLength(50)]
    [Column(TypeName = "varchar(50)")]
    //This is the subnet used by the VPN Server, e.g., 192.168.50.0/24
    public string AddressCidr { get; set; }
    
    [Required]
    [MaxLength(50)]
    [Column(TypeName = "varchar(50)")]
    //Gateway address of vpn server, e.g, 192.168.50.1/24
    public string ServerAddress { get; set; }
    
    public bool IsEnabled { get; set; } = true;
}