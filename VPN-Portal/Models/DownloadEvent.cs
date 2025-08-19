using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VPN_Portal.Authentication;
using VPN_Portal.Models.Devices;

namespace VPN_Portal.Models;

public class DownloadEvent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(50)]
    public string DownloadEventId { get; set; } = Guid.NewGuid().ToString();
    
    [MaxLength(50)]
    public string? TokenId { get; set; }
    [ForeignKey(nameof(TokenId))]
    public virtual DownloadToken? Token { get; set;}

    [MaxLength(50)]
    public string? PeerId { get; set; }
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
    public DateTime AttemptedUtc { get; set; } = DateTime.UtcNow;
    
    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime? RedeemedUtc { get; set; } = DateTime.UtcNow;

    [Required]
    [Column(TypeName = "int")]
    public DownloadOutcome Outcome { get; set; } = DownloadOutcome.Success;
}

public enum DownloadOutcome
{
    Success = 1,
    TokenExpired = 2,
    TokenAlreadyUsed = 3,
    NotFound = 4,
    NoConfigFound = 5
}
