using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VPN_Portal.Authentication;
using VPN_Portal.Data;
using VPN_Portal.Models;
using VPN_Portal.Models.Security;
using VPN_Portal.Services;

namespace VPN_Portal.Areas.Security.Services;

public class SecurityService
{
    private readonly ApplicationDbContext _context;
    private readonly SecurityCache _cache;
    private readonly SecurityActionService _actionService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;

    public SecurityService(ApplicationDbContext context,
        SecurityCache cache,
        SecurityActionService actionService,
        UserManager<ApplicationUser> userManager,
        IConfiguration config)
    {
        _context = context;
        _cache = cache;
        _actionService = actionService;
        _userManager = userManager;
        _config = config;
    }


    public async Task<List<SecurityEvent>> GetSecurityEvents() 
        => await _context.SecurityEvents
            .Include(e => e.User)
            .Include(e => e.Actions)
            .ToListAsync();
    
    
    private async Task<bool> PersistedUser(string userId)
        => await _context.Users.AnyAsync(u => u.Id == userId);

    public async Task<bool> GenerateCreateUserEvent(string userId, CreatedUserType createdBy, TakenByType takenBy = TakenByType.System, string? createKey = null)
    {
        var persisted = await PersistedUser(userId);
        if(!persisted) return false;

        var securityEvent = new SecurityEvent
        {
            EventType = EventType.CreateUser,
            UserId = userId
        };
        
        if (createdBy != CreatedUserType.Admin) securityEvent.ElevatedSeverity = ThreatSeverity.Critical;
        if(createKey != _config["CU_KEY"]) securityEvent.ElevatedSeverity = ThreatSeverity.High;

        
        switch (securityEvent.ElevatedSeverity)
        {
            case ThreatSeverity.Critical:
            {
                var res = await _actionService.PerformAccountDeletion(userId, securityEvent.EventId, takenBy);
                if (!res) return false;
                break;
            }
            case ThreatSeverity.High:
            {
                var res = await _actionService.PerformAccountDisable(userId, securityEvent.EventId, takenBy);
                if (!res) return false;
                break;
            }
            default:
                await _cache.UpdateValidUsersFile(userId);
                break;
        }

        var createUserEvent = new CreateUserEvent
        {
            EventId = securityEvent.EventId,
            CreatedBy = createdBy
        };
        
        await _context.SecurityEvents.AddAsync(securityEvent);
        await _context.CreateUserEvents.AddAsync(createUserEvent);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> GenerateAddRouterPeerEvent(string userId, string peerId, string publicKey, string? ipAddress)
    {
        var persisted = await PersistedUser(userId);
        if(!persisted) return false;
        
        var validPeer = await _context.DevicePeers
            .Include(p => p.Device)
            .AnyAsync(p => p.PeerId == peerId && p.Device!.UserId == userId);
        if(!validPeer) return false;
        
        var securityEvent = new SecurityEvent
        {
            EventType = EventType.AddRouterPeer,
            UserId = userId
        };
        var addRouterPeerEvent = new AddRouterPeerEvent
        {
            EventId = securityEvent.EventId,
            PeerId = peerId,
            PublicKey = publicKey,
            IpAssigned = ipAddress ?? "Unknown"
        };
        
        await _context.SecurityEvents.AddAsync(securityEvent);
        await _context.AddRouterPeerEvents.AddAsync(addRouterPeerEvent);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> GenerateInvalidTokenEvent(string userId, string? tokenId, string? peerId,
        DownloadOutcome outcome, DownloadTokenPurpose purpose)
    {
        var persisted = await PersistedUser(userId);
        if(!persisted) return false;
        
        var validPeer = await _context.DevicePeers.AnyAsync(p => p.PeerId == peerId && p.Device!.UserId == userId);
        if(!validPeer) return false;
        
        var securityEvent = new SecurityEvent
        {
            EventType = EventType.InvalidToken,
            UserId = userId
        };
        
        var validToken = await _context.DownloadTokens.AnyAsync(t => t.DownloadTokenId == tokenId && t.PeerId == peerId);
        var invalidTokenEvent = new InvalidTokenEvent
        {
            EventId = securityEvent.EventId,
            PeerId = peerId,
            TokenId = validToken ? tokenId : null,
            Outcome = outcome,
            Purpose = purpose
        };
        
        await _context.SecurityEvents.AddAsync(securityEvent);
        await _context.InvalidTokenEvents.AddAsync(invalidTokenEvent);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> GeneratePeerAbuseEvent()
    {
        return false;
    }

    public async Task<bool> GenerateInvalidLoginEvent(string targetUsername, TakenByType takenBy = TakenByType.System)
    {
        var user = await _userManager.FindByNameAsync(targetUsername);
        string? userId = null;
        if(user != null) userId = user.Id;

        var existingEvent = await _context.InvalidLoginAttemptEvents
            .FirstOrDefaultAsync(ila => ila.TargetUsername == targetUsername
                                        && ila.LastSeenUtc >= DateTime.UtcNow.AddMinutes(-10));
        SecurityEvent? securityEvent = null;
        if (existingEvent != null && !string.IsNullOrEmpty(userId))
        {
            securityEvent = await _context.SecurityEvents.FirstOrDefaultAsync(e => e.EventId == existingEvent.EventId);
            if(securityEvent == null) return false;
            
            var validUser = await PersistedUser(userId);
            if (validUser)
            {
                existingEvent.Attempts++;
                existingEvent.LastSeenUtc = DateTime.UtcNow;

                if (existingEvent.Attempts >= 8)
                {
                    securityEvent.ElevatedSeverity = ThreatSeverity.High;
                    _ = await _actionService.PerformAccountDisable(userId, securityEvent.EventId, takenBy);
                }
                else if (existingEvent.Attempts >= 5)
                {
                    securityEvent.ElevatedSeverity = ThreatSeverity.Medium;
                }
                
                _context.InvalidLoginAttemptEvents.Update(existingEvent);
                _context.SecurityEvents.Update(securityEvent);
                await _context.SaveChangesAsync();
                return true;
            }
        }
        
        securityEvent ??= new SecurityEvent
        {
            EventType = EventType.InvalidLoginAttempt,
            UserId = userId,
            ElevatedSeverity = string.IsNullOrEmpty(userId) ? ThreatSeverity.Medium : null,
        };
        existingEvent = new InvalidLoginAttemptEvent
        {
            EventId = securityEvent.EventId,
            TargetUsername = targetUsername,
            Attempts = 1
        };
        
        await _context.SecurityEvents.AddAsync(securityEvent);
        await _context.InvalidLoginAttemptEvents.AddAsync(existingEvent);
        await _context.SaveChangesAsync();
        return true;
    }
}