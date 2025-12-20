using Microsoft.EntityFrameworkCore;
using VPN_Portal.Authentication;
using VPN_Portal.Data;
using VPN_Portal.Models.Security;
using VPN_Portal.Services;
using VPN_Portal.Services.Config;
using VPN_Portal.Services.DNS;

namespace VPN_Portal.Areas.Security.Services;

public class SecurityActionService
{
    private readonly PeerService _peerService;
    private readonly ApplicationDbContext _context;
    private readonly VpnService _vpnService;
    private readonly FileService _publicKeys;
    private readonly FileService _privateKeys;
    private readonly FileService _configFiles;

    public SecurityActionService(ApplicationDbContext context,
        VpnService vpnService,
        PeerService peerService,
        IConfiguration config)
    {
        _peerService = peerService;
        _context = context;
        _vpnService = vpnService;

        var path = config["BASE_PATH"];
        if(string.IsNullOrEmpty(path)) throw new Exception("BASE_PATH not set in config.");
        
        _publicKeys = new FileService(path, FileService.FileType.PublicKey);
        _privateKeys = new FileService(path, FileService.FileType.PrivateKey);
        _configFiles = new FileService(path, FileService.FileType.Config);
    }

    private async Task CreateAction(string userId, string eventId, ActionType actionType, TakenByType takenBy,
        bool isReversible = true)
    {
        var action = new SecurityAction
        {
            EventId = eventId,
            UserId = userId,
            ActionType = actionType,
            TakenByType = takenBy,
            IsReversible = isReversible
        };
        await _context.SecurityActions.AddAsync(action);
        
        var securityEvent = await _context.SecurityEvents.FirstOrDefaultAsync(e => e.EventId == eventId);
        if(securityEvent == null) return;

        securityEvent.Status = actionType switch
        {
            ActionType.AccountDisabled or ActionType.PeerSoftRemoved
                => EventStatus.Acknowledged,
            ActionType.RouterPeerRemoved or ActionType.PeerHardRemoved or ActionType.AccountDeleted 
                => EventStatus.Resolved,
            ActionType.PeerRestored or ActionType.AccountEnabled
                => securityEvent.Status,
            _ => throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null)
        };

        _context.SecurityEvents.Update(securityEvent);
    }
    
    public async Task<bool> PerformAccountDeletion(string userId, string eventId, TakenByType takenBy)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if(user == null) return false;

        _context.Remove(user);
        await CreateAction(userId, eventId, ActionType.AccountDeleted, takenBy, false);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> PerformAccountDisable(string userId, string eventId, TakenByType takenBy)
    {
        var identityUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if(identityUser == null) return false;

        var user = (ApplicationUser)identityUser;
        user.IsDeactivated = true;
        
        _context.Users.Update(user);
        await CreateAction(userId, eventId, ActionType.AccountDisabled, takenBy);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PerformAccountEnable(string userId, string eventId, TakenByType takenBy)
    {
        var identityUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if(identityUser == null) return false;
        
        var user = (ApplicationUser)identityUser;
        user.IsDeactivated = false;
        
        _context.Users.Update(user);
        await CreateAction(userId, eventId, ActionType.AccountEnabled, takenBy);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PerformRouterPeerRemoved(string peerId, string userId, TakenByType takenBy,
        bool saveNow = false)
    {
        var peer = await _context.DevicePeers
            .Include(p => p.Device)
            .FirstOrDefaultAsync(p => p.PeerId == peerId && p.Device!.UserId == userId);
        if (peer == null) return false;

        //Remove peer from router:
        var res = await _vpnService.RemovePeerFromRouter(peer);
        if (!res) return false;

        //Soft delete peer for user:
        peer.IsDeleted = true;
        peer.DeletedUtc = DateTime.UtcNow;

        var events = await _context.AddRouterPeerEvents
            .Where(rp => rp.PeerId == peerId)
            .Select(rp => rp.EventId)
            .ToListAsync();
        foreach (var eventId in events)
        {
            await CreateAction(userId, eventId, ActionType.RouterPeerRemoved, takenBy, isReversible: false);
        }

        if(saveNow) await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PerformPeerSoftDelete(string peerId, string userId, string eventId, TakenByType takenBy)
    {
        var res = await _peerService.TryDeletePeer(peerId, userId);
        if(!res) return false;
        
        await CreateAction(userId, eventId, ActionType.PeerSoftRemoved, takenBy);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PerformPeerRestore(string peerId, string userId, string eventId, TakenByType takenBy)
    {
        var res = await _peerService.TryRestorePeer(peerId);
        if(!res) return false;
        
        await CreateAction(userId, eventId, ActionType.PeerRestored, takenBy);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PerformPeerHardDelete(string peerId, string userId, string eventId, TakenByType takenBy)
    {
        var peer = await _context.DevicePeers
            .Include(p => p.Device)
            .FirstOrDefaultAsync(p => p.PeerId == peerId && p.Device!.UserId == userId);
        if (peer == null) return false;
        
        //Remove from router:
        var res = await _vpnService.RemovePeerFromRouter(peer);
        if(!res) return false;

        //Remove unauthorised vpn peer events:
        var unauthVpnPeerEvents = await _context.UnauthorisedVpnPeerEvents
            .Include(up => up.SecurityEvent)
            .ThenInclude(se => se.Actions)
            .Where(up => up.PeerId == peerId && up.SecurityEvent!.UserId == userId)
            .ToListAsync();
        if (unauthVpnPeerEvents.Count > 0)
        {
            _context.SecurityActions.RemoveRange(unauthVpnPeerEvents.SelectMany(up => up.SecurityEvent.Actions));
            _context.SecurityEvents.RemoveRange(unauthVpnPeerEvents.Select(up => up.SecurityEvent));;
            _context.UnauthorisedVpnPeerEvents.RemoveRange(unauthVpnPeerEvents);
        }

        //Remove unauthorised vpn token events:
        var unauthVpnTokenEvents = await _context.UnauthorisedVpnTokenEvents
            .Include(upt => upt.SecurityEvent)
            .ThenInclude(se => se.Actions)
            .Where(upt => upt.PeerId == peerId && upt.SecurityEvent!.UserId == userId)
            .ToListAsync();
        if (unauthVpnTokenEvents.Count > 0)
        {
            _context.SecurityActions.RemoveRange(unauthVpnTokenEvents.SelectMany(upt => upt.SecurityEvent.Actions));
            _context.SecurityEvents.RemoveRange(unauthVpnTokenEvents.Select(upt => upt.SecurityEvent));
            _context.UnauthorisedVpnTokenEvents.RemoveRange(unauthVpnTokenEvents);
        }

        //Remove invalid token events:
        var invalidTokenEvents = await _context.InvalidTokenEvents
            .Include(ite => ite.SecurityEvent)
            .ThenInclude(se => se.Actions)
            .Where(ite => ite.PeerId == peerId && ite.SecurityEvent!.UserId == userId)
            .ToListAsync();
        if (invalidTokenEvents.Count > 0)
        {
            _context.SecurityActions.RemoveRange(invalidTokenEvents.SelectMany(ite => ite.SecurityEvent.Actions));
            _context.SecurityEvents.RemoveRange(invalidTokenEvents.Select(ite => ite.SecurityEvent));
            _context.InvalidTokenEvents.RemoveRange(invalidTokenEvents);
        }
        
        //Remove download events:
        var downloadEvents = await _context.DownloadEvents
            .Where(de => de.PeerId == peerId && de.UserId == userId)
            .ToListAsync();
        if (downloadEvents.Count > 0)
        {
            _context.DownloadEvents.RemoveRange(downloadEvents);   
        }
        
        //Remove tokens:
        var tokens = await _context.DownloadTokens
            .Where(dt => dt.PeerId == peerId && dt.UserId == userId)
            .ToListAsync();
        if (tokens.Count > 0)
        {
            _context.DownloadTokens.RemoveRange(tokens);   
        }
        
        //Remove peer reservations:
        var peerToReservations = await _context.PeerToReservations
            .Where(ptr => ptr.PeerId == peerId)
            .ToListAsync();
        var reservations = await _context.DnsReservations
            .Where(r => peerToReservations.Select(ptr => ptr.ReservationId).Contains(r.ReservationId))
            .ToListAsync();
        if (peerToReservations.Count > 0)
        {
            _context.PeerToReservations.RemoveRange(peerToReservations);
        }

        if (reservations.Count > 0)
        {
            _context.DnsReservations.RemoveRange(reservations);
        }
        
        //Delete Files:
        _ = _publicKeys.DeleteFile($"{peer.PeerId}.txt", peer.DeviceId);
        _ = _privateKeys.DeleteFile($"{peer.PeerId}.txt", peer.DeviceId);
        _ = _configFiles.DeleteFile($"{peer.PeerId}.conf", peer.DeviceId);
        
        //Remove peer:
        _context.DevicePeers.Remove(peer);
        await CreateAction(userId, eventId, ActionType.PeerHardRemoved, takenBy, false);
        await _context.SaveChangesAsync();
        return true;
    }
}