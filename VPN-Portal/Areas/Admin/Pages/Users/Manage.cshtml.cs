using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VPN_Portal.Areas.Admin.Models;
using VPN_Portal.Areas.Admin.Services;
using VPN_Portal.Authentication;

namespace VPN_Portal.Areas.Admin.Pages.Users;

[Authorize(Roles = SystemRoles.SystemAdmin)]
public class ManageModel : PageModel
{
    private readonly AdminService _adminService;
    private readonly ILogger<ManageModel> _logger;

    public ManageModel(AdminService adminService, ILogger<ManageModel> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }
    
    public List<UserViewModel> Users { get; set; } = new();
    
    [TempData]
    public string? SuccessMessage { get; set; }
    
    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        Users = await _adminService.GetUsers();
        Users = Users.OrderBy(u => u.Username).ToList();
        return Page();
    }
    
    public async Task<IActionResult> OnPostDeleteAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            ErrorMessage = "Invalid user ID.";
            return RedirectToPage();
        }

        var user = await _adminService.GetUser(userId);
        if (user == null)
        {
            ErrorMessage = "User not found.";
            return RedirectToPage();
        }

        var success = await _adminService.TryDeleteUser(userId);
        if (success)
        {
            _logger.LogInformation("Admin deleted user: {Username}", user.Username);
            SuccessMessage = $"User '{user.Username}' has been deleted successfully.";
        }
        else
        {
            ErrorMessage = "Failed to delete user. Please try again.";
        }

        return RedirectToPage();
    }
    
    public async Task<IActionResult> OnPostResetPasswordAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            ErrorMessage = "Invalid user ID.";
            return RedirectToPage();
        }

        var user = await _adminService.GetUser(userId);
        if (user == null)
        {
            ErrorMessage = "User not found.";
            return RedirectToPage();
        }

        var newPassword = await _adminService.TryResetPassword(userId);
        if (!string.IsNullOrEmpty(newPassword))
        {
            _logger.LogInformation("Admin reset password for user: {Username}", user.Username);
            TempData["ResetUsername"] = user.Username;
            TempData["ResetPassword"] = newPassword;
            SuccessMessage = $"Password reset successfully for user '{user.Username}'.";
        }
        else
        {
            ErrorMessage = "Failed to reset password. Please try again.";
        }

        return RedirectToPage();
    }
}