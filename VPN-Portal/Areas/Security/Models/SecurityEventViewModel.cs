using QRCoder.Extensions;
using VPN_Portal.Extensions;
using VPN_Portal.Models.Security;

namespace VPN_Portal.Areas.Security.Models;

public class SecurityEventViewModel
{
    public string EventId { get; set; }
    public string EventType { get; set; }
    public string Severity { get; set; }
    public string Status { get; set; }
    public string UserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public uint NumberOfActions { get; set; }

    public SecurityEventViewModel(SecurityEvent securityEvent)
    {
        EventId = securityEvent.EventId;
        EventType = securityEvent.EventType.ToSpacedString();
        Severity = securityEvent.ElevatedSeverity == null ? securityEvent.BaseSeverity.ToSpacedString() : securityEvent.ElevatedSeverity.ToSpacedString();
        Status = securityEvent.Status.ToSpacedString();
        UserName = securityEvent.User == null ? "Unknown" : securityEvent.User.DisplayName ?? securityEvent.User.UserName ?? "Unknown";
        CreatedAt = securityEvent.CreatedUtc.ToLocalTime();
        NumberOfActions = (uint)(securityEvent.Actions?.Count ?? 0);
    }
}