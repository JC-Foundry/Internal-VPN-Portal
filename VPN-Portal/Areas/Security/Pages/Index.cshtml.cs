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
    
    public List<SecurityEventViewModel> SecurityEvents { get; set; } = [];
    public EventType? ShownEventType { get; set; }
    public bool ShowResolved { get; set; }

    public async Task OnGet(EventType? type = null, bool showResolved = false)
    {
        ShownEventType = type;
        ShowResolved = showResolved;
        var events = await _securityService.GetSecurityEvents();
        
        // Determine which statuses to count based on showResolved flag
        var countStatuses = showResolved
            ? new[] { EventStatus.Resolved }
            : new[] { EventStatus.Open, EventStatus.Acknowledged };

        //Severity Counts:
        InfoCount = (uint)events.Count(e => (type != null ? e.EventType == type : e.EventType >= 0)
                                            && (e.ElevatedSeverity ?? e.BaseSeverity) == ThreatSeverity.Info
                                            && countStatuses.Contains(e.Status));
        LowCount = (uint)events.Count(e => (type != null ? e.EventType == type : e.EventType >= 0)
                                           && (e.ElevatedSeverity ?? e.BaseSeverity) == ThreatSeverity.Low
                                           && countStatuses.Contains(e.Status));
        MediumCount = (uint)events.Count(e => (type != null ? e.EventType == type : e.EventType >= 0)
                                              && (e.ElevatedSeverity ?? e.BaseSeverity) == ThreatSeverity.Medium
                                              && countStatuses.Contains(e.Status));
        HighCount = (uint)events.Count(e => (type != null ? e.EventType == type : e.EventType >= 0)
                                            && (e.ElevatedSeverity ?? e.BaseSeverity) == ThreatSeverity.High
                                            && countStatuses.Contains(e.Status));
        CriticalCount = (uint)events.Count(e => (type != null ? e.EventType == type : e.EventType >= 0)
                                                && (e.ElevatedSeverity ?? e.BaseSeverity) == ThreatSeverity.Critical
                                                && countStatuses.Contains(e.Status));

        //Type counts:
        CreateUserCount = (uint)events.Count(e => e.EventType == EventType.CreateUser && countStatuses.Contains(e.Status));
        AddRouterPeerCount = (uint)events.Count(e => e.EventType == EventType.AddRouterPeer && countStatuses.Contains(e.Status));
        InvalidTokenCount = (uint)events.Count(e => e.EventType == EventType.InvalidToken && countStatuses.Contains(e.Status));
        PeerAbuseCount = (uint)events.Count(e => e.EventType == EventType.PeerAbuse && countStatuses.Contains(e.Status));
        InvalidLoginCount = (uint)events.Count(e => e.EventType == EventType.InvalidLoginAttempt && countStatuses.Contains(e.Status));
        UnauthorisedPeerCount = (uint)events.Count(e => e.EventType == EventType.UnauthorisedVpnPeer && countStatuses.Contains(e.Status));
        UnauthorisedTokenCount = (uint)events.Count(e => e.EventType == EventType.UnauthorisedVpnToken && countStatuses.Contains(e.Status));

        // Filter events: show Open/Acknowledged/Suppressed by default, or Resolved if toggled
        var displayStatuses = showResolved
            ? new[] { EventStatus.Resolved }
            : new[] { EventStatus.Open, EventStatus.Acknowledged, EventStatus.Suppressed };

        SecurityEvents = events.Where(e => (e.EventType == type || type == null) && displayStatuses.Contains(e.Status))
            .Select(e => new SecurityEventViewModel(e))
            .OrderByDescending(e => e.CreatedAt)
            .ToList();
    }
}