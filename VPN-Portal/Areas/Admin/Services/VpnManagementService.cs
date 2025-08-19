using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using VPN_Portal.Data;
using VPN_Portal.Helpers;
using VPN_Portal.Models.VpnServers;

namespace VPN_Portal.Areas.Admin.Services;

public class VpnManagementService
{
    private readonly ApplicationDbContext _context;

    public VpnManagementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<VpnServer>> GetVpnServers(bool asNoTracking = true, bool includePool = true, bool includeReservations = true, bool includePeers = false)
    {
        var query = _context.VpnServers.Where(v => v.IsEnabled);
        if(includePool && !includeReservations) query = query.Include(v => v.DnsPool);
        if (includePool && includeReservations) query = query.Include(v => v.DnsPool).ThenInclude(p => p!.Reservations);
        if(includePeers) query = query.Include(v => v.Peers);
        
        if (asNoTracking) query = query.AsNoTracking();
        return await query.ToListAsync();
    }
    
    public async Task<VpnServer?> GetVpnServer(string vpnServerId, bool asNoTracking = true, bool includePool = true)
    {
        var query = _context.VpnServers.Where(v => v.IsEnabled);
        if(includePool) query = query.Include(v => v.DnsPool);
        if (asNoTracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(s => s.VpnServerId == vpnServerId);
    }

    private async Task ValidateVpnServer(bool adding, VpnServer vpnServer, ModelStateDictionary modelState)
    {
        var res = await _context.VpnServers.AnyAsync(v => v.Name == vpnServer.Name 
                                                                    && v.VpnServerId != vpnServer.VpnServerId
                                                                    && v.IsEnabled);
        if (res)
        {
            modelState.AddModelError($"Input.{nameof(vpnServer.Name)}", "A VPN server with this name already exists.");
        }

        var validCidr = IpAddressHelper.ValidateIpAddress(vpnServer.AddressCidr);
        if (!validCidr)
        {
            modelState.AddModelError($"Input.{nameof(vpnServer.AddressCidr)}", "Invalid CIDR.");
        }
        
        var validServer = IpAddressHelper.ValidateIpAddress(vpnServer.ServerAddress);
        if (!validServer)
        {
            modelState.AddModelError($"Input.{nameof(vpnServer.ServerAddress)}", "Invalid server address.");
        }
        
        var validDnsPool = await _context.DnsPools.AnyAsync(p => p.DnsPoolId == vpnServer.DnsPoolId && p.Enabled);;
        if (!validDnsPool)
        {
            modelState.AddModelError($"Input.{nameof(vpnServer.DnsPoolId)}", "Invalid DNS pool.");
        }
    }
    
    public async Task<bool> TryAddVpnServer(VpnServer vpnServer, ModelStateDictionary modelState)
    {
        await ValidateVpnServer(true, vpnServer, modelState);
        if (!modelState.IsValid) return false;
        
        _context.VpnServers.Add(vpnServer);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> TryUpdateVpnServer(VpnServer vpnServer, ModelStateDictionary modelState)
    {
        await ValidateVpnServer(false, vpnServer, modelState);
        if (!modelState.IsValid) return false;
        
        _context.VpnServers.Update(vpnServer);
        await _context.SaveChangesAsync();
        return true;
    }
}