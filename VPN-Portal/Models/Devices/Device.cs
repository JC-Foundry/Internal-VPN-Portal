using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using VPN_Portal.Authentication;

namespace VPN_Portal.Models.Devices;

[Table("Devices")]
[Index(nameof(UserId))]
[Index(nameof(CreatedUtc))]
[Index(nameof(IsRevoked))]
public class Device
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(50)]
    public string DeviceId { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; }
    
    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }
    
    [Required]
    [MaxLength(60)]
    [Column(TypeName = "nvarchar(60)")]
    public string DeviceName { get; set; }
    
    [Required]
    [Column(TypeName = "int")]
    public DeviceType DeviceType { get; set; } = DeviceType.Unknown;
    
    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    
    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime LastSeenUtc { get; set; } = DateTime.UtcNow;
    
    [Required]
    public bool IsRevoked { get; set; }
    
    public virtual ICollection<DevicePeer>? Peers { get; set; }
}

public enum DeviceType
{
    Unknown = -1,
    Desktop,
    Mobile
}