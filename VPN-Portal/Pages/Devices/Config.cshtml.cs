using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VPN_Portal.Models;
using VPN_Portal.Models.Devices;
using VPN_Portal.Services;

namespace VPN_Portal.Pages.Devices;

[Authorize]
public class ConfigModel : PageModel
{
    private readonly PeerService _peerService;
    private readonly DownloadTokenService _downloadTokenService;

    public ConfigModel(PeerService peerService, 
        DownloadTokenService downloadTokenService)
    {
        _peerService = peerService;
        _downloadTokenService = downloadTokenService;
    }

    [BindProperty(SupportsGet = true)]
    public string PeerId { get; set; }
    
    public DevicePeer? Peer { get; set; }
    
    public async Task<IActionResult> OnGetAsync(string peerId)
    {
        if (string.IsNullOrEmpty(peerId))
        {
            return RedirectToPage("/Index");
        }

        PeerId = peerId;
        Peer = await _peerService.GetPeer(peerId);
        
        if (Peer == null)
        {
            TempData["ErrorMessage"] = "Configuration not found.";
            return RedirectToPage("/Index");
        }
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostDownloadFileAsync(string peerId)
    {
        var (result, token) = await _downloadTokenService.TryCreateToken(peerId);
        if (result) return RedirectToPage("/Download", new { tokenId = token, purpose = DownloadTokenPurpose.Config });
        
        TempData["ErrorMessage"] = "Failed to download config file. Please try again.";
        return RedirectToPage("/Index");
    }
    
    public async Task<IActionResult> OnPostShowQRCodeAsync(string peerId)
    {
        var (result, token) = await _downloadTokenService.TryCreateToken(peerId, purpose: DownloadTokenPurpose.Qr);
        if (result) return RedirectToPage("/Download", new { tokenId = token, purpose = DownloadTokenPurpose.Qr });
        
        TempData["ErrorMessage"] = "Failed to create QR code. Please try again.";
        return RedirectToPage("/Index");
    }
}