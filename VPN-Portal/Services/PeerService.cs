using Microsoft.EntityFrameworkCore;
using VPN_Portal.Data;
using VPN_Portal.Models.Devices;
using VPN_Portal.Services.Config;
using VPN_Portal.Services.DNS;

namespace VPN_Portal.Services;

public class PeerService
{
    private readonly ApplicationDbContext _context;
    private readonly UserInfo _userInfo;
    private readonly ReservationService _reservationService;
    private readonly KeyGenerationService _keyGenerationService;
    private readonly VpnService _vpnService;
    private readonly VpnConfigService _vpnConfigService;

    public PeerService(ApplicationDbContext context,
        UserInfo userInfo,
        ReservationService reservationService,
        KeyGenerationService keyGenerationService,
        VpnService vpnService,
        VpnConfigService vpnConfigService)
    {
        _context = context;
        _userInfo = userInfo;
        _reservationService = reservationService;
        _keyGenerationService = keyGenerationService;
        _vpnService = vpnService;
        _vpnConfigService = vpnConfigService;
    }

    public async Task<DevicePeer?> GetPeer(string peerId, bool asNoTracking = true, bool includeDevice = true, bool includeVpnServer = true, bool includeReservations = true)
    {
        IQueryable<DevicePeer> query = _context.DevicePeers;
        if (asNoTracking) query = query.AsNoTracking();
        if (includeDevice) query = query.Include(p => p.Device);
        if (includeVpnServer) query = query.Include(p => p.VpnServer);

        if (!includeReservations) return await query.FirstOrDefaultAsync(p => p.PeerId == peerId);
        {
            var peer = await query.FirstOrDefaultAsync(p => p.PeerId == peerId);
            if (peer == null) return null;
            
            peer.PeerToReservations = await _context.PeerToReservations
                .Include(ptr => ptr.DnsReservation)
                .ThenInclude(r => r.DnsPool)
                .Where(ptr => ptr.PeerId == peerId)
                .ToListAsync();
            return peer;
        }

    }
    
    public async Task<(bool Result, string PeerId)> TryCreatePeer(string deviceId, string vpnServerId)
    {
        var valid = await _context.Devices.AnyAsync(d => d.DeviceId == deviceId
                                                               && d.UserId == _userInfo.UserId);
        if(!valid) return (false, string.Empty);
        
        valid = await _context.VpnServers.AnyAsync(v => v.VpnServerId == vpnServerId);
        if(!valid) return (false, string.Empty);
        
        valid = await _context.DevicePeers.AnyAsync(dp => dp.VpnServerId == vpnServerId && dp.DeviceId == deviceId);
        if(valid) return (false, string.Empty);
        
        var peer = new DevicePeer
        {
            DeviceId = deviceId,
            VpnServerId = vpnServerId
        };
        
        await _context.DevicePeers.AddAsync(peer);
        await _context.SaveChangesAsync();
        return (true, peer.PeerId);
    }

    public async Task<bool> TryDeletePeer(string peerId)
    {
        var peer = await _context.DevicePeers.FirstOrDefaultAsync(p => p.PeerId == peerId);
        if (peer == null) return false;
        
        //Remove from router:
         var res = await _vpnService.RemovePeerFromRouter(peer);
         if(!res) return false;
        
        //Delete config:
        res = _vpnConfigService.TryDeleteConfig(peer);
        if(!res) return false;
        
        //Delete keys:
        res = _keyGenerationService.TryDeleteKeys(peer);
        if(!res) return false;
        
        //Delete reservations:
        res = await _reservationService.TryRelease(peer);
        if(!res) return false;
        
        //Clear tokens:
        var tokens = await _context.DownloadTokens
            .Where(t => t.UserId == _userInfo.UserId && t.PeerId == peerId)
            .ToListAsync();
        if (tokens.Count != 0)
        {
            _context.DownloadTokens.RemoveRange(tokens);
        }

        //Clear events:
        var events = await _context.DownloadEvents
            .Where(de => de.PeerId == peerId && de.UserId == _userInfo.UserId)
            .ToListAsync();
        if (events.Count != 0)
        {
            _context.DownloadEvents.RemoveRange(events);
        }
        
        _context.DevicePeers.Remove(peer);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ProvisionPeerConfig(string peerId)
    {
        var peer = await GetPeer(peerId);
        if (peer == null) return false;
        
        //Reserve IP:
        var reservation = await _reservationService.TryReserve(peer.DeviceId, peer.PeerId, peer.VpnServer!.DnsPoolId);
        if(reservation == null) return false;
        
        peer = await GetPeer(peerId);
        if (peer == null) return false;

        //Generate keys:
        _ = await _keyGenerationService.GetKeys(peer);
        
        //Add to router:
        var res = await _vpnService.AddPeerToRouter(peer);
        if(!res) return false;
        
        //Create config:
        res = await _vpnConfigService.TryCreateConfig(peer);
        return res;
    }
}