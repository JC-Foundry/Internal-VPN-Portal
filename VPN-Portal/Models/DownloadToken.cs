using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using VPN_Portal.Authentication;
using VPN_Portal.Models.Devices;

namespace VPN_Portal.Models;

[Table("DownloadTokens")]
[Index(nameof(PeerId))]
[Index(nameof(ExpiresUtc), nameof(UsedUtc))]
public class DownloadToken
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(50)]
    public string DownloadTokenId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [MaxLength(50)]
    public string PeerId { get; set; }
    [ForeignKey(nameof(PeerId))]
    public virtual DevicePeer? Peer { get; set; }
    
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser? User { get; set; }
    
    
    [Required]
    [Column(TypeName = "int")]
    public DownloadTokenPurpose Purpose { get; set; } = DownloadTokenPurpose.Config;
    
    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    
    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime ExpiresUtc { get; set; }


    [Column(TypeName = "datetime2")]
    public DateTime? UsedUtc { get; set; }

    /// <summary>
    /// Will return a 404 not found after 5hrs from expiry
    /// </summary>
    [NotMapped]
    public DateTime NotFoundUtc => ExpiresUtc.AddHours(5);
    
    [NotMapped]
    public bool IsExpired => DateTime.UtcNow > ExpiresUtc;

    [NotMapped]
    public bool IsUsed => UsedUtc != null || IsCleared;
    
    public bool IsCleared { get; set; } = false;
}

public enum DownloadTokenPurpose
{
    Config = 1,
    Qr = 2
}