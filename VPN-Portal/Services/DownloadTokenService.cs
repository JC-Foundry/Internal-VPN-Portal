using Microsoft.EntityFrameworkCore;
using QRCoder;
using VPN_Portal.Areas.Security.Services;
using VPN_Portal.Data;
using VPN_Portal.Models;
using VPN_Portal.Services.Config;

namespace VPN_Portal.Services;

public class DownloadTokenService
{
    private readonly ApplicationDbContext _context;
    private readonly UserInfo _userInfo;
    private readonly VpnConfigService _vpnConfigService;
    private readonly PeerService _peerService;
    private readonly SecurityService _securityService;

    public DownloadTokenService(ApplicationDbContext context,
        UserInfo userInfo,
        VpnConfigService vpnConfigService,
        PeerService peerService,
        SecurityService securityService)
    {
        _context = context;
        _userInfo = userInfo;
        _vpnConfigService = vpnConfigService;
        _peerService = peerService;
        _securityService = securityService;
    }

    public async Task<(bool Result, string? TokenId)> TryCreateToken(string peerId, DownloadTokenPurpose purpose = DownloadTokenPurpose.Config)
    {
        var peer = await _peerService.GetPeer(peerId);
        if (peer == null) return (false, null);

        var (config, _) = await _vpnConfigService.GetConfig(peer);
        if (config == null) return (false, null);

        var token = new DownloadToken
        {
            UserId = _userInfo.UserId,
            PeerId = peerId,
            ExpiresUtc = DateTime.UtcNow.AddMinutes(5),
            Purpose = purpose,
        };
        await _context.DownloadTokens.AddAsync(token);
        await _context.SaveChangesAsync();
        return (true, token.DownloadTokenId);
    }

    public async Task<string> CreateDownloadEvent(string? peerId, string? tokenId, 
        DownloadTokenPurpose purpose, DownloadOutcome outcome)
    {
        var downloadEvent = new DownloadEvent
        {
            UserId = _userInfo.UserId,
            PeerId = peerId,    //null if NotFound on token
            TokenId = tokenId,  //null if NotFound on token
            Purpose = purpose,
            AttemptedUtc = DateTime.UtcNow,
            Outcome = outcome,
            RedeemedUtc = outcome == DownloadOutcome.Success ? DateTime.UtcNow : null
        };
        await _context.DownloadEvents.AddAsync(downloadEvent);
        await _context.SaveChangesAsync();
        return downloadEvent.DownloadEventId;
    }

    public async Task<DownloadRedeemResult> RedeemToken(string tokenId, DownloadTokenPurpose purpose)
    {
        var now = DateTime.UtcNow;
        
        var token = await _context.DownloadTokens
            .Include(t => t.Peer)
            .ThenInclude(p => p!.Device)
            .FirstOrDefaultAsync(t => t.DownloadTokenId == tokenId 
                                      && t.UserId == _userInfo.UserId);

        //Token missing
        if (token?.Peer == null)
        {
            _ = await _securityService.GenerateInvalidTokenEvent(_userInfo.UserId, null, null, DownloadOutcome.NotFound, purpose);
            _ = await CreateDownloadEvent(null, null, purpose, DownloadOutcome.NotFound);
            return new DownloadRedeemResult("Token not found.")
            {
                Outcome = DownloadOutcome.NotFound,
                TokenId = tokenId,
                Utc = now
            };
        }
        
        //Token passed NotFoundUtc
        if (now > token.NotFoundUtc)
        {
            _ = await _securityService.GenerateInvalidTokenEvent(_userInfo.UserId, tokenId, token.PeerId, DownloadOutcome.NotFound, token.Purpose);
            _ = await CreateDownloadEvent(token.PeerId, tokenId, token.Purpose, DownloadOutcome.NotFound);
            return new DownloadRedeemResult("Token not found.")
            {
                Outcome = DownloadOutcome.NotFound,
                Purpose = token.Purpose,
                TokenId = tokenId,
                PeerId = token.PeerId,
                Utc = now
            };
        }

        //Token expired
        if (now > token.ExpiresUtc)
        {
            _ = await _securityService.GenerateInvalidTokenEvent(_userInfo.UserId, tokenId, token.PeerId, DownloadOutcome.TokenExpired, token.Purpose);
            _ = await CreateDownloadEvent(token.PeerId, tokenId, token.Purpose, DownloadOutcome.TokenExpired);
            return new DownloadRedeemResult("Token expired.")
            {
                Outcome = DownloadOutcome.TokenExpired,
                Purpose = token.Purpose,
                TokenId = tokenId,
                PeerId = token.PeerId,
                Utc = now
            };
        }

        //Token used
        if (token.IsUsed)
        {
            _ = await _securityService.GenerateInvalidTokenEvent(_userInfo.UserId, tokenId, token.PeerId, DownloadOutcome.TokenAlreadyUsed, token.Purpose);
            _ = await CreateDownloadEvent(token.PeerId, tokenId, token.Purpose, DownloadOutcome.TokenAlreadyUsed);
            return new DownloadRedeemResult("Token already used.")
            {
                Outcome = DownloadOutcome.TokenAlreadyUsed,
                Purpose = token.Purpose,
                TokenId = tokenId,
                PeerId = token.PeerId,
                Utc = now
            };
        }
        
        //Get config
        var (configBytes, config) = await _vpnConfigService.GetConfig(token.Peer);
        if (config == null || configBytes == null)
        {
            _ = await _securityService.GenerateInvalidTokenEvent(_userInfo.UserId, tokenId, token.PeerId, DownloadOutcome.NoConfigFound, token.Purpose);
            _ = await CreateDownloadEvent(token.PeerId, tokenId, token.Purpose, DownloadOutcome.NoConfigFound);
            return new DownloadRedeemResult("Failed to fetch config.")
            {
                Outcome = DownloadOutcome.NoConfigFound,
                Purpose = token.Purpose,
                PeerId = token.PeerId,
                TokenId = tokenId,
                Utc = now
            };
        }
        
        //Use token:
        token.UsedUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        //Return success:
        var fileName = purpose == DownloadTokenPurpose.Config ? $"{token.Peer.Device?.DeviceName.Replace('-', '_')}_VPN.conf" : null;
        _ = await CreateDownloadEvent(token.PeerId, tokenId, token.Purpose, DownloadOutcome.Success);
        return new DownloadRedeemResult(fileName, config, configBytes)
        {
            Outcome = DownloadOutcome.Success,
            Purpose = token.Purpose,
            PeerId = token.PeerId,
            TokenId = tokenId,
            Utc = now
        };
    }
}

public class DownloadRedeemResult
{
    public DownloadOutcome Outcome { get; set; }
    public DownloadTokenPurpose Purpose { get; set; }
    public string? TokenId { get; set; }
    public string? PeerId { get; set; }
    public DateTime Utc { get; set; }
    public string? FileName { get; set; }
    public string? Config { get; set; }
    public byte[]? ConfigBytes { get; set; }
    public string? ErrorMessage { get; set; }

    public DownloadRedeemResult(string? fileName, string config, byte[] configBytes)
    {
        FileName = fileName;
        Config = config;
        ConfigBytes = configBytes;
    }

    public DownloadRedeemResult(string errorMessage)
    {
        ErrorMessage = errorMessage;   
    }
}