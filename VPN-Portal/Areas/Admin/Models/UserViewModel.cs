using VPN_Portal.Authentication;

namespace VPN_Portal.Areas.Admin.Models;

public class UserViewModel
{
    public string UserId { get; set; }
    public string? Username { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    
    public DateTime? LastLogin { get; set; }
    public uint? MaxPeers { get; set; }
    
    public List<string?> Roles { get; set; }

    public UserViewModel()
    {
    }

    public UserViewModel(ApplicationUser user, List<string?> roles)
    {
        UserId = user.Id;
        Username = user.UserName;
        DisplayName = user.DisplayName;
        Email = user.Email;
        PhoneNumber = user.PhoneNumber;
        LastLogin = user.LastLogin;
        MaxPeers = user.MaxPeers;
        Roles = roles;
    }
}