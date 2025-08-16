using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VPN_Portal.Models.Devices;
using VPN_Portal.Services;

namespace VPN_Portal.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly DeviceService _deviceService;

    public IndexModel(ILogger<IndexModel> logger,
        DeviceService deviceService)
    {
        _logger = logger;
        _deviceService = deviceService;
    }

    public List<Device> Devices { get; set; } = new();
    
    public async Task<IActionResult> OnGetAsync()
    {
        Devices = await _deviceService.GetDevices(asNoTracking: true);
        return Page();
    }
    
    public async Task<IActionResult> OnPostRevokeAsync(string deviceId)
    {
        if (string.IsNullOrEmpty(deviceId))
        {
            TempData["Error"] = "Invalid device ID.";
            return RedirectToPage();
        }
        
        var success = await _deviceService.RevokeDevice(deviceId);
        if (success)
        {
            TempData["Success"] = "Device revoked successfully.";
            _logger.LogInformation("Device {DeviceId} revoked successfully", deviceId);
        }
        else
        {
            TempData["Error"] = "Failed to revoke device. Please try again.";
            _logger.LogWarning("Failed to revoke device {DeviceId}", deviceId);
        }
        
        return RedirectToPage();
    }
    
    public async Task<IActionResult> OnPostDeletePeerAsync(string peerId)
    {
        if (string.IsNullOrEmpty(peerId))
        {
            TempData["Error"] = "Invalid peer ID.";
            return RedirectToPage();
        }
        
        // TODO: Implement peer deletion in DeviceService
        TempData["Info"] = "Peer deletion functionality will be implemented soon.";
        _logger.LogInformation("Attempted to delete peer {PeerId}", peerId);
        
        return RedirectToPage();
    }
    
    public async Task<IActionResult> OnPostDownloadConfigAsync(string peerId)
    {
        if (string.IsNullOrEmpty(peerId))
        {
            TempData["Error"] = "Invalid peer ID.";
            return RedirectToPage();
        }
        
        // TODO: Implement config generation and download
        TempData["Info"] = "Configuration download functionality will be implemented soon.";
        _logger.LogInformation("Attempted to download config for peer {PeerId}", peerId);
        
        // For now, redirect back to the page
        // In the future, this should return a File result with the WireGuard config
        return RedirectToPage();
    }
}