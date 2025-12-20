using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VPN_Portal.Pages;

[Authorize]
public class WireGuardGuideModel : PageModel
{
    public void OnGet()
    {
        // This is a static guide page, no data needed
    }
}