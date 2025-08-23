using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using tik4net.Objects.User;
using VPN_Portal.Areas.Admin.Models;
using VPN_Portal.Areas.Security.Services;
using VPN_Portal.Authentication;
using VPN_Portal.Data;
using VPN_Portal.Helpers;
using VPN_Portal.Models.Security;

namespace VPN_Portal.Areas.Admin.Services;

public class AdminService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SecurityService _securityService;
    private readonly IConfiguration _config;

    public AdminService(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        SecurityService securityService,
        IConfiguration config)
    {
        _context = context;
        _userManager = userManager;
        _securityService = securityService;
        _config = config;
    }

    private async Task<List<string?>> GetRoles(ApplicationUser user)
    {
        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == user.Id)
            .Select(ur => ur.RoleId)
            .ToListAsync();
            
        return await _context.Roles
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Name)
            .OrderBy(r => r)
            .ToListAsync();
    }

    public async Task<(int TotalUsers, int TotalDevices, int TotalStandardUsers, int TotalReadOnlyUsers)> 
        GetStats(List<UserViewModel> users)
    {
        var td = await _context.Devices.Where(d => !d.IsRevoked).CountAsync();
        var tsu = users.Count(u => u.Roles.Contains(SystemRoles.StandardUser));
        var trou = users.Count(u => u.Roles.Contains(SystemRoles.ReadOnlyUser));
        return (users.Count, td, tsu, trou);
    }
    
    public async Task<List<UserViewModel>> GetUsers()
    {
        var users = await _context.Users.ToListAsync();
        var viewModels = new List<UserViewModel>();
        foreach (var user in users.Select(u => (ApplicationUser)u))
        {
            var roles = await GetRoles(user);
            viewModels.Add(new UserViewModel(user, roles));
        }
        
        return viewModels;
    }

    public async Task<List<ApplicationUser>> GetUsers(bool asNoTracking)
    {
        var query = _context.Users.AsQueryable();
        if (asNoTracking) query = query.AsNoTracking();
        return await query.Select(u => (ApplicationUser)u).ToListAsync();
    }

    public async Task<UserViewModel?> GetUser(string userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return null;
        
        var roles = await GetRoles((ApplicationUser)user);
        return new UserViewModel((ApplicationUser)user, roles);
    }

    public async Task<bool> TryChangeMaxPeers(string userId, int maxPeers)
    {
        var identityUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (identityUser == null) return false;

        var user = (ApplicationUser)identityUser;
        uint? max = maxPeers switch
        {
            < 1 => null,
            _ => (uint)maxPeers
        };
        
        user.MaxPeers = max;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string> TryResetPassword(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return string.Empty;

        var newPassword = PasswordHelper.GenerateStrongPassword(16);

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            return string.Empty;
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.UpdateSecurityStampAsync(user);

        return newPassword;
    }
    
    public async Task<bool> TryDeleteUser(string userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;
        
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TryAddRole(string userId, string roleName)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if(user == null) return false;
        
        var res = await _userManager.AddToRoleAsync((ApplicationUser)user, roleName);
        return res.Succeeded;
    }

    public async Task<bool> TryRemoveRole(string userId, string roleName)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if(user == null) return false;
        
        var res = await _userManager.RemoveFromRoleAsync((ApplicationUser)user, roleName);
        return res.Succeeded;
    }

    public async Task<(bool Success, string Password)> TryCreateUser(string username, string email, string? displayName = null)
    {
        var existingUser = await _userManager.FindByNameAsync(username);
        if (existingUser != null) return (false, string.Empty);
        existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null) return (false, string.Empty);
        
        var user = new ApplicationUser
        {
            Email = email,
            UserName = username,
            DisplayName = displayName,
            EmailConfirmed = true,
            TwoFactorEnabled = false,
            LastLogin = null
        };
        
        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded) return (false, string.Empty);
        
        var password = PasswordHelper.GenerateStrongPassword(16);
        result = await _userManager.AddPasswordAsync(user, password);
        if (!result.Succeeded) return (false, string.Empty);
        
        result = await _userManager.AddToRoleAsync(user, SystemRoles.StandardUser);
        //Create security event:
        _ = await _securityService.GenerateCreateUserEvent(user.Id, CreatedUserType.Admin, TakenByType.System, _config["CU_KEY"]);
        return (result.Succeeded, result.Succeeded ? password : string.Empty);
    }
}