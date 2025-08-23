using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VPN_Portal.Areas.Security.Models;
using VPN_Portal.Areas.Security.Services;
using VPN_Portal.Authentication;
using VPN_Portal.Models.Security;

namespace VPN_Portal.Areas.Security.Pages;

[Authorize(Roles = SystemRoles.SystemAdmin)]
public class Index : PageModel
{
    private readonly SecurityService _securityService;

    public Index(SecurityService securityService)
    {
        _securityService = securityService;
    }

    public uint InfoCount { get; set; } 
    public uint LowCount { get; set; } 
    public uint MediumCount { get; set; }
    public uint HighCount { get; set; }
    public uint CriticalCount { get; set; }
    public uint CreateUserCount { get; set; }
    public uint AddRouterPeerCount { get; set; }
    public uint InvalidTokenCount { get; set; }
    public uint PeerAbuseCount { get; set; }
    public uint InvalidLoginCount { get; set; }
    public uint UnauthorisedPeerCount { get; set; }
    public uint UnauthorisedTokenCount { get; set; }
    public uint RoleElevationCount { get; set; }
    
    public List<SecurityEventViewModel> SecurityEvents { get; set; } = [];
    
    public async Task OnGet()
    {
        var events = await _securityService.GetSecurityEvents();
        //Severity Counts:
        InfoCount = (uint)events.Count(e => (e.ElevatedSeverity ?? e.BaseSeverity) == ThreatSeverity.Info && (e.Status is EventStatus.Open or EventStatus.Acknowledged));
        LowCount = (uint)events.Count(e => (e.ElevatedSeverity ?? e.BaseSeverity) == ThreatSeverity.Low && (e.Status is EventStatus.Open or EventStatus.Acknowledged));
        MediumCount = (uint)events.Count(e => (e.ElevatedSeverity ?? e.BaseSeverity) == ThreatSeverity.Medium && (e.Status is EventStatus.Open or EventStatus.Acknowledged));
        HighCount = (uint)events.Count(e => (e.ElevatedSeverity ?? e.BaseSeverity) == ThreatSeverity.High && (e.Status is EventStatus.Open or EventStatus.Acknowledged));
        CriticalCount = (uint)events.Count(e => (e.ElevatedSeverity ?? e.BaseSeverity) == ThreatSeverity.Critical && (e.Status is EventStatus.Open or EventStatus.Acknowledged));
        
        //Type counts:
        CreateUserCount = (uint)events.Count(e => e is { EventType: EventType.CreateUser, Status: EventStatus.Open or EventStatus.Acknowledged });
        AddRouterPeerCount = (uint)events.Count(e => e is { EventType: EventType.AddRouterPeer, Status: EventStatus.Open or EventStatus.Acknowledged });
        InvalidTokenCount = (uint)events.Count(e => e is { EventType: EventType.InvalidToken, Status: EventStatus.Open or EventStatus.Acknowledged });
        PeerAbuseCount = (uint)events.Count(e => e is { EventType: EventType.PeerAbuse, Status: EventStatus.Open or EventStatus.Acknowledged });
        InvalidLoginCount = (uint)events.Count(e => e is { EventType: EventType.InvalidLoginAttempt, Status: EventStatus.Open or EventStatus.Acknowledged });
        UnauthorisedPeerCount = (uint)events.Count(e => e is { EventType: EventType.UnauthorisedVpnPeer, Status: EventStatus.Open or EventStatus.Acknowledged });
        UnauthorisedTokenCount = (uint)events.Count(e => e is { EventType: EventType.UnauthorisedVpnToken, Status: EventStatus.Open or EventStatus.Acknowledged });
        RoleElevationCount = (uint)events.Count(e => e is { EventType: EventType.RoleElevation, Status: EventStatus.Open or EventStatus.Acknowledged });
        
        SecurityEvents = events.Where(e => e.CreatedUtc >= DateTime.UtcNow.AddDays(-30))
            .Select(e => new SecurityEventViewModel(e))
            .OrderByDescending(e => e.CreatedAt)
            .ToList();
    }
}