using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using VPN_Portal.Authentication.UserClaims;
using VPN_Portal.Services;

namespace VPN_Portal.Middleware;

public static class UserInfoMiddlewareExtensions
{
    public static IApplicationBuilder UseUserInfo(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<UserInfoMiddleware>();
    }
}

public class UserInfoMiddleware
{
    private readonly RequestDelegate _next;

    public UserInfoMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var userInfo = (UserInfo)context.RequestServices.GetRequiredService(typeof(UserInfo));
        var io = context.RequestServices.GetRequiredService<IOptions<IdentityOptions>>();
        if (!userInfo.IsSetup)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                userInfo.UserName = "System";
            }
            else
            {
                userInfo.UserName = context.User.Identity.Name;
                userInfo.DisplayName = context.User.FindFirst(UserClaims.DisplayNameClaim)?.Value;
                var lastLogin = context.User.FindFirst(UserClaims.LastLoginClaim)?.Value;
                userInfo.LastLogin = lastLogin != null ? DateTime.Parse(lastLogin) : DateTime.Now;
                
                var maxDeviceCountStr = context.User.FindFirst(UserClaims.MaxPeerCountClaim)?.Value;
                uint? maxDeviceCount = null;
                if (maxDeviceCountStr?.ToUpper() != "UNLIMITED")
                {
                    var res = uint.TryParse(maxDeviceCountStr, out var mdc);
                    if(res) maxDeviceCount = mdc;
                }
                userInfo.MaxDeviceCount = maxDeviceCount;
                
                userInfo.UserName = context.User.FindFirst(io.Value.ClaimsIdentity.EmailClaimType)?.Value;

                // var tidClaim = context.User.FindFirst(UserClaims.TenantId);
                // if (tidClaim == null)
                // {
                //     await context.SignOutAsync("Identity.Application");
                //     await _next(context);
                //     return;
                // }
                //
                // userInfo.TenantId = int.Parse(tidClaim.Value);
                userInfo.UserId = context.User.FindFirst(io.Value.ClaimsIdentity.UserIdClaimType)?.Value;

                if (string.IsNullOrWhiteSpace(userInfo.UserId))
                {
                    throw new InvalidOperationException("No userid in userinfo");
                }
            }

            userInfo.IsSetup = true;
        }

        await _next(context);
    }
}