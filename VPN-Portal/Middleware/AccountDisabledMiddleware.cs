using VPN_Portal.Authentication.UserClaims;

namespace VPN_Portal.Middleware;

public static class AccountDisabledMiddlewareExtensions
{
    public static IApplicationBuilder UseAccountDisabled(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AccountDisabledMiddleware>();
    }
}

public class AccountDisabledMiddleware
{
    private readonly RequestDelegate _next;
    
    public AccountDisabledMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var disabledClaim = context.User.FindFirst(UserClaims.IsDisabled)?.Value;
            if (!string.IsNullOrEmpty(disabledClaim) && bool.TryParse(disabledClaim, out var disabled) && disabled)
            {
                var path = context.Request.Path.Value ?? string.Empty;
                
                // Skip redirect for static files and allowed paths
                if (IsStaticFile(path) 
                    || path.StartsWith("/Identity/Account/Disabled", StringComparison.OrdinalIgnoreCase)
                    || path.StartsWith("/Identity/Account/Logout", StringComparison.OrdinalIgnoreCase))
                {
                    await _next(context);
                    return;
                }
                
                // Redirect to disabled page for all other requests
                context.Response.Redirect("/Identity/Account/Disabled");
                return;
            }
        }
        
        await _next(context);
    }
    
    private bool IsStaticFile(string path)
    {
        // Check if path is for static resources
        if (path.StartsWith("/css/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/js/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/images/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/fonts/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/_framework/", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/_content/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
        
        // Check for common static file extensions
        var extension = Path.GetExtension(path);
        if (!string.IsNullOrEmpty(extension))
        {
            var staticExtensions = new[] { ".css", ".js", ".jpg", ".jpeg", ".png", ".gif", ".svg", ".ico", ".woff", ".woff2", ".ttf", ".eot", ".map", ".json" };
            return staticExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
        }
        
        return false;
    }
}