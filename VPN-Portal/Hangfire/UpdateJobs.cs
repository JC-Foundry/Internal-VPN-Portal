using VPN_Portal.Services.Config;

namespace VPN_Portal.Hangfire;

public class UpdateJobs
{
    private readonly VpnService _vpnService;
    private readonly ILogger<UpdateJobs> _logger;

    public UpdateJobs(VpnService vpnService,
        ILogger<UpdateJobs> logger)
    {
        _vpnService = vpnService;
        _logger = logger;
    }

    public async Task UpdateLastHandshakes()
    {
        var res = await _vpnService.UpdateLastHandshakes();
        if(res) return;
        
        _logger.LogError("Failed to update last handshakes");
    }
}