using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VPN_Portal.Models.Devices;
using VPN_Portal.Services;

namespace VPN_Portal.Pages.Devices;

[Authorize]
public class ConfigModel : PageModel
{
    private readonly PeerService _peerService;

    public ConfigModel(PeerService peerService)
    {
        _peerService = peerService;
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
        // TODO: Implement file download logic
        // This will redirect to the Download page with appropriate parameters
        
        return RedirectToPage("/Download", new { peerId = peerId, type = "file" });
    }
    
    public async Task<IActionResult> OnPostShowQRCodeAsync(string peerId)
    {
        // TODO: Implement QR code display logic
        // This will redirect to the Download page with appropriate parameters
        
        return RedirectToPage("/Download", new { peerId = peerId, type = "qr" });
    }
}