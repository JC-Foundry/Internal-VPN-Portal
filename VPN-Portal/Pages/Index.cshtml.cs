using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VPN_Portal.Areas.Security.Services;
using VPN_Portal.Models.Devices;
using VPN_Portal.Models.Security;
using VPN_Portal.Services;

namespace VPN_Portal.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly DeviceService _deviceService;
    private readonly PeerService _peerService;
    private readonly SecurityActionService _securityActionService;
    private readonly UserInfo _userInfo;

    public IndexModel(ILogger<IndexModel> logger,
        DeviceService deviceService,
        PeerService peerService,
        SecurityActionService securityActionService,
        UserInfo userInfo)
    {
        _logger = logger;
        _deviceService = deviceService;
        _peerService = peerService;
        _securityActionService = securityActionService;
        _userInfo = userInfo;
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
            TempData["ErrorMessage"] = "Invalid device ID.";
            return RedirectToPage();
        }
        
        var success = await _deviceService.RevokeDevice(deviceId);
        if (success)
        {
            TempData["SuccessMessage"] = "Device revoked successfully.";
            _logger.LogInformation("Device {DeviceId} revoked successfully", deviceId);
        }
        else
        {
            TempData["ErrorMessage"] = "Failed to revoke device. Please try again.";
            _logger.LogWarning("Failed to revoke device {DeviceId}", deviceId);
        }
        
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeletePeerAsync(string peerId)
    {
        if (string.IsNullOrEmpty(peerId))
        {
            TempData["ErrorMessage"] = "Invalid peer ID.";
            return RedirectToPage();
        }

        var res = await _peerService.TryDeletePeer(peerId);
        if (res)
        {
            await _securityActionService.PerformUserDeletedPeer(peerId, _userInfo.UserId);
            //_ = await _securityActionService.PerformRouterPeerRemoved(peerId, _userInfo.UserId, TakenByType.User, true);
            TempData["SuccessMessage"] = "Peer deleted successfully.";
            _logger.LogInformation("Peer {PeerId} deleted successfully", peerId);
            return RedirectToPage();
        }

        TempData["ErrorMessage"] = "Failed to delete peer. Please try again.";
        _logger.LogWarning("Failed to delete peer {PeerId}", peerId);

        return RedirectToPage();
    }
}