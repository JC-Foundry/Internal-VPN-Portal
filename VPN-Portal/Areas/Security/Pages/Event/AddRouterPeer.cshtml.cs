using VPN_Portal.Areas.Security.Services;
using VPN_Portal.Models.Security;

namespace VPN_Portal.Areas.Security.Pages.Event;

public class AddRouterPeer : EventPageModel<AddRouterPeerEvent>
{
    public AddRouterPeer(SecurityService securityService, SecurityActionService securityActionService)
        : base(securityService, securityActionService)
    {
    }
}
