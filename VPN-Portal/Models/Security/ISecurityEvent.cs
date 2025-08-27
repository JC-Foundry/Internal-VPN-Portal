namespace VPN_Portal.Models.Security;

public interface ISecurityEvent
{
    string EventId { get; set; }
    SecurityEvent SecurityEvent { get; set; }
}