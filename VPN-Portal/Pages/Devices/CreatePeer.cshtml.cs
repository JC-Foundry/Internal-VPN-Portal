using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VPN_Portal.Areas.Admin.Services;
using VPN_Portal.Authentication;
using VPN_Portal.Data;
using VPN_Portal.Models.Devices;
using VPN_Portal.Services;

namespace VPN_Portal.Pages.Devices;

[Authorize]
public class CreatePeerModel : PageModel
{
    private readonly DeviceService _deviceService;
    private readonly VpnManagementService _vpnManagementService;
    private readonly PeerService _peerService;

    public CreatePeerModel(
        DeviceService deviceService,
        VpnManagementService vpnManagementService,
        PeerService peerService)
    {
        _deviceService = deviceService;
        _vpnManagementService = vpnManagementService;
        _peerService = peerService;
    }

    public Device Device { get; set; }
    public List<object> VpnServersData { get; set; }

    private async Task<IActionResult?> SetupPage(string deviceId)
    {
        var device = await _deviceService.GetDevice(deviceId);
        if (device == null) return NotFound();
        
        Device = device;
        await LoadVpnServers();
        return null;
    }
    

    public async Task<IActionResult> OnGetAsync(string deviceId)
    {
        var res = await SetupPage(deviceId);
        return res ?? Page();
    }

    public async Task<IActionResult> OnPostAsync(string deviceId, string vpnServerId)
    {
        var res = await SetupPage(deviceId);
        if (res != null) return res;
        
        var (success, peerId) = await _peerService.TryCreatePeer(deviceId, vpnServerId);
        if (!success)
        {
            TempData["ErrorMessage"] = $"Failed to create peer for device '{Device.DeviceName}'. Please try again.";
            return RedirectToPage("/Index");
        }

        success = await _peerService.ProvisionPeerConfig(peerId);
        if (!success)
        {
            TempData["ErrorMessage"] = $"Failed to provision peer config for device '{Device.DeviceName}'. Please try again.";
            return RedirectToPage("/Index");
        }
        
        TempData["SuccessMessage"] = $"Peer created successfully for device '{Device.DeviceName}'.";
        return RedirectToPage($"/Devices/Config/{peerId}");
    }

    private async Task LoadVpnServers()
    {
        var vpnServers = (await _vpnManagementService.GetVpnServers(includePeers: true))
            .Where(v => !v.Peers!.Select(p => p.DeviceId).Contains(Device.DeviceId));
        
        VpnServersData = vpnServers.Select(server => 
        {
            var reservedCount = server.DnsPool?.Reservations?.Count(r => r.IsActive) ?? 0;
            var totalCapacity = server.DnsPool != null ? 
                (server.DnsPool.MaxHost - server.DnsPool.MinHost + 1) : 0;
            var usagePercentage = totalCapacity > 0 ? 
                (reservedCount * 100 / totalCapacity) : 0;

            return new
            {
                VpnServerId = server.VpnServerId,
                Name = server.Name,
                EndpointHost = server.EndpointHost,
                DnsPoolRange = server.DnsPool?.GetRange() ?? "N/A",
                UsageText = $"{reservedCount}/{totalCapacity}",
                UsagePercentage = usagePercentage,
                Reserved = reservedCount,
                Total = totalCapacity
            };
        }).Cast<object>().ToList();
    }
}