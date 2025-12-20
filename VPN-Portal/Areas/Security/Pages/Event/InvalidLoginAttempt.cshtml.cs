using VPN_Portal.Areas.Security.Services;
using VPN_Portal.Models.Security;

namespace VPN_Portal.Areas.Security.Pages.Event;

public class InvalidLoginAttempt : EventPageModel<InvalidLoginAttemptEvent>
{
    public InvalidLoginAttempt(SecurityService securityService, SecurityActionService securityActionService)
        : base(securityService, securityActionService)
    {
    }
}
