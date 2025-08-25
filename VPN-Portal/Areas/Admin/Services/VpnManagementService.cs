using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using VPN_Portal.Data;
using VPN_Portal.Helpers;
using VPN_Portal.Models.VpnServers;
using VPN_Portal.Services;

namespace VPN_Portal.Areas.Admin.Services;

public class VpnManagementService
{
    private readonly ApplicationDbContext _context;
    private readonly UserInfo _userInfo;

    public VpnManagementService(ApplicationDbContext context, 
        UserInfo userInfo)
    {
        _context = context;
        _userInfo = userInfo;
    }

    public async Task<List<VpnServer>> GetVpnServers(bool asNoTracking = true, bool includePool = true, bool includeReservations = true, bool includePeers = false, 
        bool excludeFullServers = false, bool filterVpnUsers = false)
    {
        var query = _context.VpnServers.Where(v => v.IsEnabled);
        if(includePool && !includeReservations) query = query.Include(v => v.DnsPool);
        if (includePool && includeReservations) query = query.Include(v => v.DnsPool).ThenInclude(p => p!.Reservations);
        if(includePeers) query = query.Include(v => v.Peers);

        if (excludeFullServers && includeReservations && includePool) 
        {
            query = query.Where(v => (v.DnsPool!.Reservations!.Count(r => r.ReleasedUtc == null) < (v.DnsPool!.MaxHost - v.DnsPool!.MinHost) + 1) || v.DnsPool!.Reservations!.Count == 0);
        }
        
        if (asNoTracking) query = query.AsNoTracking();
        
        var servers = await query.ToListAsync();
        if(!filterVpnUsers) return servers;
        
        var authedVpnServers = new List<VpnServer>();
        foreach (var  v in servers)
        {
            var userIds = await _context.VpnServerUsers
                .Where(vu => vu.VpnServerId == v.VpnServerId)
                .Select(vu => vu.UserId)
                .ToListAsync();
            if (userIds.Contains(_userInfo.UserId))
            {
                authedVpnServers.Add(v);
            }
        }
        
        return authedVpnServers;
    }
    
    public async Task<VpnServer?> GetVpnServer(string vpnServerId, bool asNoTracking = true, bool includePool = true)
    {
        var query = _context.VpnServers.Where(v => v.IsEnabled);
        if(includePool) query = query.Include(v => v.DnsPool);
        if (asNoTracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(s => s.VpnServerId == vpnServerId);
    }

    public async Task<List<string>> GetVpnServerUserIds(string vpnServerId)
    {
        var query = _context.VpnServerUsers.Where(vu => vu.VpnServerId == vpnServerId);
        return await query.Select(vu => vu.UserId).ToListAsync();
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
    
    public async Task<bool> TryAddVpnServer(VpnServer vpnServer, List<string> userIds, ModelStateDictionary modelState)
    {
        await ValidateVpnServer(true, vpnServer, modelState);
        if (!modelState.IsValid) return false;
        if(userIds.Count == 0) return false;
        
        var vpnUsers = userIds
            .Select(userId => new VpnServerUser
            {
                VpnServerId = vpnServer.VpnServerId, 
                UserId = userId
            }).ToList();

        await _context.VpnServerUsers.AddRangeAsync(vpnUsers);
        await _context.VpnServers.AddAsync(vpnServer);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> TryUpdateVpnServer(VpnServer vpnServer, List<string> userIds, ModelStateDictionary modelState)
    {
        await ValidateVpnServer(false, vpnServer, modelState);
        if (!modelState.IsValid) return false;
        if(userIds.Count == 0) return false;

        var currentUserIds = await _context.VpnServerUsers
            .Where(vu => vu.VpnServerId == vpnServer.VpnServerId)
            .ToListAsync();
        var userIdsToRemove = currentUserIds.Where(u => !userIds.Contains(u.UserId)).ToList();
        var userIdsToAdd = userIds.Where(u => currentUserIds.All(cu => cu.UserId != u)).ToList();

        if (userIdsToRemove.Count > 0)
        {
            _context.VpnServerUsers.RemoveRange(userIdsToRemove);
        }
        
        var vpnUsers = userIdsToAdd
            .Select(userId => new VpnServerUser
            {
                VpnServerId = vpnServer.VpnServerId, 
                UserId = userId
            }).ToList();

        await _context.VpnServerUsers.AddRangeAsync(vpnUsers);
        _context.VpnServers.Update(vpnServer);
        await _context.SaveChangesAsync();
        return true;
    }
}