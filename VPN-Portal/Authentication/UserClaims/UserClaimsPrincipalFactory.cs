using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace VPN_Portal.Authentication.UserClaims;

public class UserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var defaultClaims = await base.GenerateClaimsAsync(user);
        
        defaultClaims.AddClaim((new Claim(UserClaims.DisplayNameClaim, user.DisplayName ?? "")));
        defaultClaims.AddClaim((new Claim(UserClaims.LastLoginClaim, user.LastLogin?.ToString("G") ?? string.Empty)));
        defaultClaims.AddClaim((new Claim(UserClaims.MaxDeviceCountClaim, user.MaxDevices.ToString() ?? "UNLIMITED")));

        return defaultClaims;
    }

    public UserClaimsPrincipalFactory(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<IdentityOptions> options)
        : base (userManager, roleManager, options)
    {
    }
}