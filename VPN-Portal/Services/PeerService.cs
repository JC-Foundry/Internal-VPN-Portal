using System.Security.Cryptography.X509Certificates;
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

    public async Task<List<DevicePeer>> GetUserPeers(string userId, bool asNoTracking = true, bool includeVpnServer = true)
    {
        var query = _context.DevicePeers
            .Include(p => p.Device)
            .Include(p => p.PeerToReservations)
            .ThenInclude(ptr => ptr.DnsReservation)
            .Where(p => p.Device!.UserId == userId);
        if (asNoTracking) query = query.AsNoTracking();
        if (includeVpnServer) query = query.Include(p => p.VpnServer);
        
        return await query.Where(p => !p.IsDeleted).OrderByDescending(p => p.CreatedUtc).ToListAsync();
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
        
        valid = await _context.DevicePeers.AnyAsync(dp => dp.VpnServerId == vpnServerId && dp.DeviceId == deviceId && !dp.IsDeleted);
        if(valid) return (false, string.Empty);

        var server = await _context.VpnServers
            .Include(v => v.DnsPool)
            .FirstOrDefaultAsync(v => v.VpnServerId == vpnServerId);
        var usedReservations = await _context.DnsReservations
            .CountAsync(r => r.DnsPoolId == server!.DnsPoolId && r.ReleasedUtc == null);
        var maxRes = (server!.DnsPool!.MaxHost - server!.DnsPool!.MinHost) + 1;
        if(usedReservations >= maxRes) return (false, string.Empty);
        
        var peer = new DevicePeer
        {
            DeviceId = deviceId,
            VpnServerId = vpnServerId
        };
        
        await _context.DevicePeers.AddAsync(peer);
        await _context.SaveChangesAsync();
        return (true, peer.PeerId);
    }

    public async Task<bool> TryDeletePeer(string peerId, string? userId = null)
    {
        var peer = await _context.DevicePeers.FirstOrDefaultAsync(p => p.PeerId == peerId);
        if (peer == null) return false;
        
        //Remove from router:
         var res = await _vpnService.RemovePeerFromRouter(peer);
         if(!res) return false;
        
        //Release reservations:
        res = await _reservationService.TryRelease(peer);
        if(!res) return false;
        
        //Clear tokens:
        var uid = userId ?? _userInfo.UserId;
        var tokens = await _context.DownloadTokens
            .Where(t => t.UserId == uid && t.PeerId == peerId)
            .ToListAsync();
        foreach (var token in tokens)
        {
            token.IsCleared = true;
            _context.DownloadTokens.Update(token);   
        }
        
        peer.IsDeleted = true;
        _context.DevicePeers.Update(peer);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ProvisionPeerConfigResult> ProvisionPeerConfig(string peerId)
    {
        var peer = await GetPeer(peerId);
        if (peer == null) return new ProvisionPeerConfigResult("Peer not found.");;
        
        //Reserve IP:
        var reservation = await _reservationService.TryReserve(peer.DeviceId, peer.PeerId, peer.VpnServer!.DnsPoolId);
        if(reservation == null) return new ProvisionPeerConfigResult("Failed to reserve IP.");;
        
        peer = await GetPeer(peerId);
        if (peer == null) return new ProvisionPeerConfigResult("Failed to reload peer.");

        //Generate keys:
        var (publicKey, _) = await _keyGenerationService.GetKeys(peer);
        
        //Add to router:
        var res = await _vpnService.AddPeerToRouter(peer);
        if(!res) return new ProvisionPeerConfigResult("Failed to add peer to the router.");
        
        //Create config:
        res = await _vpnConfigService.TryCreateConfig(peer);
        return res ? new ProvisionPeerConfigResult(peer, publicKey) : new ProvisionPeerConfigResult("Failed to create config.");
    }
}

public class ProvisionPeerConfigResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public DevicePeer? Peer { get; set; }
    public string? PublicKey { get; set; }
    public string? AllowedIp { get; set; }

    public ProvisionPeerConfigResult(string error)
    {
        Success = false;
        Error = error;
    }

    public ProvisionPeerConfigResult(DevicePeer peer, string publicKey)
    {
        Success = true;
        Peer = peer;
        PublicKey = publicKey;
        AllowedIp = peer.AssignedIp();
    }
}