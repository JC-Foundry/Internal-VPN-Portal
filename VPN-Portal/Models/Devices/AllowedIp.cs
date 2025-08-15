using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VPN_Portal.Models.Devices;

[Table("AllowedIps")]
[Index(nameof(PeerId))]
[Index(nameof(IpAddress))]
public class AllowedIp
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(50)]
    public string AllowedIpId { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(50)]
    public string PeerId { get; set; }
    
    [ForeignKey(nameof(PeerId))]
    public virtual DevicePeer? Peer { get; set; }
    
    [Required]
    [MaxLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string IpAddress { get; set; }
}