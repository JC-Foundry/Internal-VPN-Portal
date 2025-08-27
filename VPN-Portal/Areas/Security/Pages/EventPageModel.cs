using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VPN_Portal.Areas.Security.Services;
using VPN_Portal.Models.Security;

namespace VPN_Portal.Areas.Security.Pages;

public class EventPageModel<T> : PageModel
    where T : class, ISecurityEvent
{
    protected readonly SecurityService _securityService;
    protected readonly SecurityActionService _securityActionService;

    public EventPageModel(SecurityService securityService,
        SecurityActionService securityActionService)
    {
        _securityService = securityService;
        _securityActionService = securityActionService;
    }
    
    public SecurityEvent BaseEvent { get; set; }  
    public T  SecurityEvent { get; set; }
    public List<SecurityAction> SecurityActions { get; set; }

    protected async Task<bool> SetupPage(string eventId)
    {
        var baseEvent = await _securityService.GetSecurityEvent(eventId);
        if (baseEvent == null) return false;

        var specificEvent = await _securityService.GetSpecificSecurityEvent<T>(eventId);
        if(specificEvent == null) return false;

        BaseEvent = baseEvent;
        SecurityEvent = specificEvent;
        SecurityActions = baseEvent.Actions.ToList();
        return true;
    }

    public async Task<IActionResult> OnGet(string eventId)
    {
        var res = await SetupPage(eventId);
        return res ? Page() : NotFound();  
    }
}