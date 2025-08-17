using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VPN_Portal.Areas.Admin.Services;
using VPN_Portal.Authentication;
using VPN_Portal.Models.VpnServers;

namespace VPN_Portal.Areas.Admin.Pages.DNS;

[Authorize(Roles = SystemRoles.SystemAdmin)]
public class PoolsModel : PageModel
{
    private readonly DnsManagementService _dnsManagementService;

    public PoolsModel(DnsManagementService dnsManagementService)
    {
        _dnsManagementService = dnsManagementService;
    }

    public List<DnsPool> DnsPools { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        DnsPools = await _dnsManagementService.GetDnsPools();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string poolId)
    {
        if (string.IsNullOrEmpty(poolId))
        {
            TempData["ErrorMessage"] = "Invalid pool ID.";
            return RedirectToPage();
        }

        var pool = await _dnsManagementService.GetDnsPool(poolId, asNoTracking: false);
        if (pool == null)
        {
            TempData["ErrorMessage"] = "DNS pool not found.";
            return RedirectToPage();
        }

        // Set Enabled to false instead of deleting
        pool.Enabled = false;
        await _dnsManagementService.TryUpdateDnsPool(pool, pool.SecondOctet, ModelState);
        
        TempData["SuccessMessage"] = $"DNS pool '{pool.Name}' has been disabled successfully.";
        return RedirectToPage();
    }
}