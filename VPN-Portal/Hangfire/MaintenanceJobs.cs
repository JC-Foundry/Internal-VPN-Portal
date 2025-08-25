using Microsoft.EntityFrameworkCore;
using VPN_Portal.Data;

namespace VPN_Portal.Hangfire;

public class MaintenanceJobs
{
    private readonly ApplicationDbContext _context;

    public MaintenanceJobs(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CleanExpiredTokens()
    {
        var oldTokens = await _context.DownloadTokens
            .Where(t => t.CreatedUtc < DateTime.UtcNow.AddDays(-30))
            .ToListAsync();
        
        _context.DownloadTokens.RemoveRange(oldTokens);
        await _context.SaveChangesAsync();
    }

    public async Task CleanOldDownloadEvents()
    {
        var oldEvents = await _context.DownloadEvents
            .Where(d => d.AttemptedUtc < DateTime.UtcNow.AddDays(-30))
            .ToListAsync();
        
        _context.DownloadEvents.RemoveRange(oldEvents);
        await _context.SaveChangesAsync();
    }
}