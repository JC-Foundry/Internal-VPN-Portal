using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using VPN_Portal.Authentication;

namespace VPN_Portal.Models.VpnServers;

[PrimaryKey(nameof(UserId), nameof(VpnServerId))]
public class VpnServerUser
{
    public string UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } 
    
    public string VpnServerId { get; set; }
    [ForeignKey(nameof(VpnServerId))]
    public VpnServer VpnServer { get; set; }
}