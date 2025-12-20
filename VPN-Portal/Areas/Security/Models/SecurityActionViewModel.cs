using VPN_Portal.Extensions;
using VPN_Portal.Models.Security;

namespace VPN_Portal.Areas.Security.Models;

public class SecurityActionViewModel
{
    public string ActionId { get; set; }
    public string EventId { get; set; }
    public string EventType { get; set; }
    public string ActionType { get; set; }
    public string TakenBy { get; set; }
    public string UserName { get; set; }
    public string? UserId { get; set; }
    public string CreatedAt { get; set; }
    public bool IsReversible { get; set; }

    public SecurityActionViewModel(SecurityAction action)
    {
        ActionId = action.ActionId;
        EventId = action.EventId;
        EventType = action.SecurityEvent?.EventType.ToSpacedString() ?? "Unknown";
        ActionType = action.ActionType.ToSpacedString();
        TakenBy = action.TakenByType.ToSpacedString();
        UserName = action.SecurityEvent?.User?.DisplayName ?? action.SecurityEvent?.User?.UserName ?? "Unknown";
        UserId = action.UserId;
        CreatedAt = action.CreatedUtc.ToLocalTime().ToString("g");
        IsReversible = action.IsReversible;
    }
}
