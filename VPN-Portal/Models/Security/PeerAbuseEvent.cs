using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VPN_Portal.Models.Security;

public class PeerAbuseEvent
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [MaxLength(50)]
    public string EventId { get; set; }
    [ForeignKey(nameof(EventId))]
    public virtual SecurityEvent SecurityEvent { get; set; }
    
    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime StartTimeUtc { get; set; } = DateTime.UtcNow;
    
    [Required]
    [Column(TypeName = "datetime2")]
    public DateTime LastSeenUtc { get; set; } = DateTime.UtcNow;
    
    [Required]
    public uint PeersMade { get; set; }
    
    [Required]
    public string PeerIds { get; set; }

    [NotMapped]
    public const string Delimiter = "--/--";
}