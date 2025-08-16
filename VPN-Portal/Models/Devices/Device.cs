using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
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
    public DeviceType DeviceType { get; set; } = DeviceType.Other;
    
    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    
    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime LastSeenUtc { get; set; } = DateTime.UtcNow;
    
    [Required]
    public bool IsRevoked { get; private set; }

    public void RevokeDevice()
    {
        IsRevoked = true;
    }

    public async Task<bool> UnRevokeDevice(UserManager<ApplicationUser>? userManager = null, ApplicationUser? user = null)
    {
        var isAdmin = false;
        if (userManager != null && user != null)
        {
            isAdmin = await userManager.IsInRoleAsync(user, SystemRoles.SystemAdmin);
        }
        
        //Can only un-revoke if admin:
        if(!isAdmin) return false;
        IsRevoked = false;
        return true;
    }
    
    public virtual ICollection<DevicePeer>? Peers { get; set; }
}

public enum DeviceType
{
    Desktop,
    Laptop,
    Tablet,
    Mobile,
    Other
}