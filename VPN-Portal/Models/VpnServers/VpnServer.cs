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
    public string AddressCidr { get; set; }
    
    [Required]
    [MaxLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string NetworkAddress { get; set; }
}