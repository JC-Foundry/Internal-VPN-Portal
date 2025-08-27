using Microsoft.AspNetCore.Mvc.RazorPages;
using VPN_Portal.Areas.Security.Services;
using VPN_Portal.Models.Security;

namespace VPN_Portal.Areas.Security.Pages.Event;

public class CreateUser : EventPageModel<CreateUserEvent>
{
    public CreateUser(SecurityService securityService, SecurityActionService securityActionService) 
        : base(securityService, securityActionService)
    {
    }

    
}