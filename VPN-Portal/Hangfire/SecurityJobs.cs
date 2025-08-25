using Microsoft.EntityFrameworkCore;
using VPN_Portal.Areas.Security.Services;
using VPN_Portal.Data;
using VPN_Portal.Models.Security;

namespace VPN_Portal.Hangfire;

public class SecurityJobs
{
    private readonly SecurityService _securityService;
    private readonly SecurityCache _securityCache;
    private readonly ApplicationDbContext _context;

    public SecurityJobs(SecurityService securityService,
        SecurityCache securityCache,
        ApplicationDbContext context)
    {
        _securityService = securityService;
        _securityCache = securityCache;
        _context = context;
    }

    public async Task CheckUnauthorisedUsers()
    {
        var users = await _context.Users
            .Select(u => u.Id).ToListAsync();
        foreach (var uid in users)
        {
            var valid = await _securityCache.ValidateUser(uid);
            if(valid) continue;

            await _securityService.GenerateCreateUserEvent(uid, CreatedUserType.Unknown);
        }
    }

    public async Task CheckPeerAbuse()
    {
        var peerGroup = await _context.DevicePeers
            .Include(p => p.Device)
            .Where(p => p.IsDeleted && p.DeletedUtc > DateTime.UtcNow.AddMinutes(-10))
            .GroupBy(p => p.Device!.UserId)
            .ToListAsync();

        foreach (var pg in peerGroup)
        {
            var count = pg.Count();
            var peerIds = pg.Select(p => p.PeerId).ToArray();
            if(count >= 5) await _securityService.GeneratePeerAbuseEvent(pg.Key, (uint)count, peerIds);
        }
    }
}