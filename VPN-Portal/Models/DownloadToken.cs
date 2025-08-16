using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
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
    public string PeerId { get; set; } = default!;
    [ForeignKey(nameof(PeerId))]
    public virtual DevicePeer? Peer { get; set; }
    
    [Required]
    [Column(TypeName = "int")]
    public DownloadTokenPurpose Purpose { get; set; } = DownloadTokenPurpose.Config;

    [Required]
    [Column(TypeName = "nvarchar(500)")]
    public string ConfigPath { get; set; }
    
    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime ExpiresUtc { get; set; }
    
    [Required]
    public bool OneTime { get; set; } = true;

    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime2")]
    public DateTime? UsedUtc { get; set; }

    [NotMapped]
    public bool IsExpired => DateTime.UtcNow > ExpiresUtc;

    [NotMapped]
    public bool IsUsed => UsedUtc != null;
}

public enum DownloadTokenPurpose
{
    Config,
    Qr
}