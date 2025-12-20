using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VPN_Portal.Areas.Security.Models;
using VPN_Portal.Areas.Security.Services;
using VPN_Portal.Authentication;

namespace VPN_Portal.Areas.Security.Pages.Actions;

[Authorize(Roles = SystemRoles.SystemAdmin)]
public class Index : PageModel
{
    private readonly SecurityService _securityService;

    public Index(SecurityService securityService)
    {
        _securityService = securityService;
    }

    public List<SecurityActionViewModel> SecurityActions { get; set; } = [];

    public async Task OnGet()
    {
        var actions = await _securityService.GetSecurityActions();
        SecurityActions = actions.Select(a => new SecurityActionViewModel(a)).ToList();
    }
}
