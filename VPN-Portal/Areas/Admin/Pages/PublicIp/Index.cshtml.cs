using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VPN_Portal.Authentication;
using VPN_Portal.Services.Config;

namespace VPN_Portal.Areas.Admin.Pages.PublicIp;

[Authorize(Roles = SystemRoles.SystemAdmin)]
public class IndexModel : PageModel
{
    private static readonly ConcurrentDictionary<string, DateTime> AccessTokens = new();
    private const int TokenExpiryMinutes = 5;

    private readonly VpnService _vpnService;
    private readonly IConfiguration _config;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(VpnService vpnService, IConfiguration config, ILogger<IndexModel> logger)
    {
        _vpnService = vpnService;
        _config = config;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string? Token { get; set; }

    [BindProperty]
    public string? Password { get; set; }

    public string? PublicIp { get; set; }
    public bool ShowPasswordForm { get; set; } = true;
    public DateTime? TokenExpiry { get; set; }

    public async Task<IActionResult> OnGet()
    {
        // Clean expired tokens first thing
        CleanExpiredTokens();

        // Check if we have a valid token
        if (string.IsNullOrEmpty(Token) || !AccessTokens.TryGetValue(Token, out var expiry)) 
            return Page();
        
        if (expiry > DateTime.UtcNow)
        {
            // Token is valid, fetch the public IP
            PublicIp = await _vpnService.GetPublicIp();
            ShowPasswordForm = false;
            TokenExpiry = expiry;

            if (string.IsNullOrEmpty(PublicIp))
            {
                ModelState.AddModelError(string.Empty, "Failed to retrieve public IP from router. Please check router connection.");
            }

            return Page();
        }

        // Token expired, remove it
        AccessTokens.TryRemove(Token, out _);

        // No valid token, show password form
        return Page();
    }

    public IActionResult OnPost()
    {
        var correctPassword = _config["PublicIpPassword"];

        if (string.IsNullOrEmpty(correctPassword))
        {
            _logger.LogError("PublicIpPassword not configured in appsettings");
            ModelState.AddModelError(string.Empty, "Server configuration error. Please contact administrator.");
            return Page();
        }

        if (Password == correctPassword)
        {
            // Generate new token
            var token = Guid.NewGuid().ToString();
            var expiry = DateTime.UtcNow.AddMinutes(TokenExpiryMinutes);
            AccessTokens[token] = expiry;

            _logger.LogInformation("Public IP access granted. Token expires at {Expiry}", expiry);

            // Redirect to GET with token
            return RedirectToPage(new { Token = token });
        }

        ModelState.AddModelError(string.Empty, "Invalid password. Access denied.");
        return Page();
    }

    private void CleanExpiredTokens()
    {
        var now = DateTime.UtcNow;
        var expired = AccessTokens.Where(x => x.Value < now).ToList();

        foreach (var token in expired)
        {
            AccessTokens.TryRemove(token.Key, out _);
        }

        if (expired.Count > 0)
        {
            _logger.LogDebug("Cleaned {Count} expired public IP access tokens", expired.Count);
        }
    }
}
