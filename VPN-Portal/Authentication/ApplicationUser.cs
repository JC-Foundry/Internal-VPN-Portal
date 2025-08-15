using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VPN_Portal.Models.Devices;

namespace VPN_Portal.Authentication;

[Table("AspNetUsers")]
[Index(nameof(LastLogin))]
public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string? DisplayName { get; set; }
    
    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime LastLogin { get; set; }
    
    [Range(0, int.MaxValue)]
    public uint? MaxDevices { get; set; }
    
    public virtual ICollection<Device> Devices { get; set; }
}