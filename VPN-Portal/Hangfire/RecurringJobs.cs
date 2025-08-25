using Hangfire;

namespace VPN_Portal.Hangfire;

public class RecurringJobs
{
    private readonly TimeZoneInfo _tz = TimeZoneInfo.Local;
    
    public void RegisterJobs()
    {
        RecurringJob.AddOrUpdate<UpdateJobs>("UPDATE-LastHandshake",
            x => x.UpdateLastHandshakes(), 
            "*/2 * * * *",
            new RecurringJobOptions{TimeZone = _tz});
        
        RecurringJob.AddOrUpdate<MaintenanceJobs>("MAINTENANCE-ExpiredTokens",
            x => x.CleanExpiredTokens(), 
            Cron.Daily,
            new RecurringJobOptions{TimeZone = _tz});
        
        RecurringJob.AddOrUpdate<MaintenanceJobs>("MAINTENANCE-OldDownloads",
            x => x.CleanOldDownloadEvents(),
            Cron.Daily,
            new RecurringJobOptions{TimeZone = _tz});
        
        RecurringJob.AddOrUpdate<SecurityJobs>("SECURITY-NewUsers",
            x => x.CheckUnauthorisedUsers(),
            Cron.Minutely,
            new RecurringJobOptions{TimeZone = _tz});
        
        RecurringJob.AddOrUpdate<SecurityJobs>("SECURITY-PeerAbuse",
            x => x.CheckPeerAbuse(),
            "*/5 * * * *",
            new RecurringJobOptions{TimeZone = _tz});
    }
}