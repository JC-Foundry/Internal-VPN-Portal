using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VPN_Portal.Areas.Admin.Models;
using VPN_Portal.Areas.Admin.Services;
using VPN_Portal.Authentication;

namespace VPN_Portal.Areas.Admin.Pages.Users;

[Authorize(Roles = SystemRoles.SystemAdmin)]
public class IndexModel : PageModel
{
    private readonly AdminService _adminService;

    public IndexModel(AdminService adminService)
    {
        _adminService = adminService;
    }
    
    public int TotalUsers { get; set; }
    public int TotalDevices { get; set; }
    public int TotalStandardUsers { get; set; }
    public int TotalReadOnlyUsers { get; set; }
    
    public List<UserViewModel> RecentUsers { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var users = await _adminService.GetUsers();
        RecentUsers = users.OrderByDescending(u => u.LastLogin).Take(10).ToList();
        (TotalUsers, TotalDevices, TotalStandardUsers, TotalReadOnlyUsers) = await _adminService.GetStats(users);
        return Page();
    }
}
