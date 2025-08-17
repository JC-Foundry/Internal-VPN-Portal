using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VPN_Portal.Areas.Admin.Services;
using VPN_Portal.Authentication;
using VPN_Portal.Models.VpnServers;

namespace VPN_Portal.Areas.Admin.Pages.VPN;

[Authorize(Roles = SystemRoles.SystemAdmin)]
public class IndexModel : PageModel
{
    private readonly VpnManagementService _vpnManagementService;

    public IndexModel(VpnManagementService vpnManagementService)
    {
        _vpnManagementService = vpnManagementService;
    }

    public List<VpnServer> VpnServers { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        VpnServers = await _vpnManagementService.GetVpnServers(includeReservations: false);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string serverId)
    {
        if (string.IsNullOrEmpty(serverId))
        {
            TempData["ErrorMessage"] = "Invalid server ID.";
            return RedirectToPage();
        }

        var server = await _vpnManagementService.GetVpnServer(serverId, asNoTracking: false);
        if (server == null)
        {
            TempData["ErrorMessage"] = "VPN server not found.";
            return RedirectToPage();
        }

        // Set IsEnabled to false instead of deleting
        server.IsEnabled = false;
        await _vpnManagementService.TryUpdateVpnServer(server, ModelState);
        
        TempData["SuccessMessage"] = $"VPN server '{server.Name}' has been disabled successfully.";
        return RedirectToPage();
    }
}