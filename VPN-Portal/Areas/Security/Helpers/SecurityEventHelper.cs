using VPN_Portal.Models.Security;

namespace VPN_Portal.Areas.Security.Helpers;

public static class SecurityEventHelper
{
    /// <summary>
    /// Tries to extract PeerId from a security event.
    /// Returns null if the event doesn't have a PeerId or it's null.
    /// For PeerAbuseEvent, returns the delimited string of all PeerIds.
    /// </summary>
    public static string? TryGetPeerId(ISecurityEvent securityEvent)
    {
        return securityEvent switch
        {
            AddRouterPeerEvent addRouterPeer => addRouterPeer.PeerId,
            InvalidTokenEvent invalidToken => invalidToken.PeerId,
            UnauthorisedVpnPeerEvent unauthorisedPeer => unauthorisedPeer.PeerId,
            UnauthorisedVpnTokenEvent unauthorisedToken => unauthorisedToken.PeerId,
            PeerAbuseEvent peerAbuse => peerAbuse.PeerIds, // Returns the full delimited string
            _ => null
        };
    }
}
