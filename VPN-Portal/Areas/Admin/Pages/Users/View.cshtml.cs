using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using VPN_Portal.Areas.Admin.Models;
using VPN_Portal.Areas.Admin.Services;
using VPN_Portal.Authentication;
using VPN_Portal.Extensions;
using VPN_Portal.Models.Devices;
using VPN_Portal.Services;

namespace VPN_Portal.Areas.Admin.Pages.Users;

[Authorize(Roles = SystemRoles.SystemAdmin)]
public class View : PageModel
{
    private readonly AdminService _adminService;
    private readonly DeviceService _deviceService;
    private readonly PeerService _peerService;

    public View(AdminService adminService, 
        DeviceService deviceService,
        PeerService peerService)
    {
        _adminService = adminService;
        _deviceService = deviceService;
        _peerService = peerService;
    }
    
    public UserViewModel User { get; set; }
    public List<SelectListItem> AddRoles { get; set; }
    public List<SelectListItem> RemoveRoles { get; set; }
    public List<Device> UserDevices { get; set; } = new();
    public List<DevicePeer> UserPeers { get; set; } = new();
    
    [BindProperty]
    public string SelectedRole { get; set; } = string.Empty;
    
    [BindProperty]
    public int MaxPeers { get; set; }

    private async Task<IActionResult?> SetupPage(string userId)
    {
        var user = await _adminService.GetUser(userId);
        if(user == null) return NotFound();
        
        User = user;
        AddRoles = ConstExtensions.GetAllConsts<SystemRoles>()
            .Where(r => !user.Roles.Contains(r.Key))
            .Select(r => new SelectListItem(r.Key, r.Value.ToString()))
            .ToList();
        RemoveRoles = ConstExtensions.GetAllConsts<SystemRoles>()
            .Where(r => user.Roles.Contains(r.Key))
            .Select(r => new SelectListItem(r.Key, r.Value.ToString()))
            .ToList();
        
        // Get user's devices
        UserDevices = await _deviceService.GetDevicesForUser(userId, includeRevoked: false);
        UserPeers = await _peerService.GetUserPeers(userId);
        
        return null;
    }
    
    public async Task<IActionResult> OnGetAsync(string userId)
    {
        var res = await SetupPage(userId);
        return res ?? Page();
    }

    public async Task<IActionResult> OnPostUpdateMaxPeersAsync(string userId)
    {
        var res = await SetupPage(userId);
        if (res != null) return res;
        
        ModelState.Remove("SelectedRole");
        if(!ModelState.IsValid) return Page();
        
        var success = await _adminService.TryChangeMaxPeers(userId, MaxPeers);
        if (success)
        {
            TempData["SuccessMessage"] = $"Maximum number of peers has been updated successfully.";
            return RedirectToPage();
        }
        
        TempData["ErrorMessage"] = "Failed to update maximum number of peers. Please try again.";
        return Page();   
    }

    public async Task<IActionResult> OnPostAddRoleAsync(string userId)
    {
        var res = await SetupPage(userId);
        if(res != null) return res;
        
        if(!ModelState.IsValid) return Page();
        
        var success = await _adminService.TryAddRole(userId, SelectedRole);
        if(success)
        {
            TempData["SuccessMessage"] = $"Role '{SelectedRole}' has been added successfully.";
            return RedirectToPage();
        }

        TempData["ErrorMessage"] = "Failed to add role. Please try again.";
        return Page();
    }
    
    public async Task<IActionResult> OnPostRemoveRoleAsync(string userId)
    {
        var res = await SetupPage(userId);
        if(res != null) return res;
        
        if(!ModelState.IsValid) return Page();
        
        var success = await _adminService.TryRemoveRole(userId, SelectedRole);
        if (success)
        {
            TempData["SuccessMessage"] = $"Role '{SelectedRole}' has been removed successfully.";
            return RedirectToPage();
        }
        
        TempData["ErrorMessage"] = "Failed to remove role. Please try again.";
        return Page();
    }
    
    public async Task<IActionResult> OnPostResetPasswordAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "Invalid user ID.";
            return RedirectToPage();
        }

        var user = await _adminService.GetUser(userId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToPage();
        }

        var newPassword = await _adminService.TryResetPassword(userId);
        if (!string.IsNullOrEmpty(newPassword))
        {
            TempData["ResetUsername"] = user.Username;
            TempData["ResetPassword"] = newPassword;
            TempData["SuccessMessage"] = $"Password reset successfully for user '{user.Username}'.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to reset password. Please try again.";
        }

        return RedirectToPage();
    }
    
    public async Task<IActionResult> OnPostDeleteAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            TempData["ErrorMessage"] = "Invalid user ID.";
            return RedirectToPage("/Users/Manage", new { area = "Admin" });
        }

        var user = await _adminService.GetUser(userId);
        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToPage("/Users/Manage", new { area = "Admin" });
        }

        var success = await _adminService.TryDeleteUser(userId);
        if (success)
        {
            TempData["SuccessMessage"] = $"User '{user.Username}' has been deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to delete user. Please try again.";
        }

        return RedirectToPage("/Users/Manage", new { area = "Admin" });
    }
}