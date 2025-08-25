using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using tik4net;
using tik4net.Objects;
using VPN_Portal.Areas.Security.Services;
using VPN_Portal.Data;
using VPN_Portal.Helpers;
using VPN_Portal.Models.Devices;

namespace VPN_Portal.Services.Config;

public class VpnService
{
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<VpnService> _logger;
    private readonly FileService _publicKeyFiles;

    public VpnService(IConfiguration config,
        ApplicationDbContext context,
        ILogger<VpnService> logger)
    {
        _config = config;
        _context = context;
        _logger = logger;
        var path = _config["BASE_PATH"];
        if(string.IsNullOrEmpty(path)) throw new Exception("BASE_PATH not set");
        
        _publicKeyFiles = new FileService(path, FileService.FileType.PublicKey);
    }
    
    public async Task<bool> AddPeerToRouter(DevicePeer peer)
    {
        try
        {
            var publicKey = await _publicKeyFiles.GetFileText($"{peer.PeerId}.txt", peer.DeviceId);
            if(string.IsNullOrEmpty(publicKey)) return false;
        
            var allowedIp = peer.AssignedIp();
            if(string.IsNullOrEmpty(allowedIp)) return false;
            var valid = IpAddressHelper.ValidateIpAddress(allowedIp);
            if(!valid) return false;
        
            using var connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
            await connection.OpenAsync(_config["ROS:IP"], _config["ROS:PORTAL-Username"], _config["ROS:PORTAL-Password"]);

            var cmd = connection.CreateCommandAndParameters("/interface/wireguard/peers/add",
                "interface", peer.VpnServer?.InterfaceName ?? "wg-vpn",
                "public-key", publicKey,
                "allowed-address", $"{allowedIp}/32",
                "responder", "yes",
                "comment", $"Device '{peer.Device!.DeviceName}' ({peer.DeviceId}) peer: {peer.PeerId}",
                "persistent-keepalive", "25");
        
            cmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding peer to router");
            return false;
        }
    }

    public async Task<bool> RemovePeerFromRouter(DevicePeer peer)
    {
        try
        {
            var publicKey = await _publicKeyFiles.GetFileText($"{peer.PeerId}.txt", peer.DeviceId);
            if(string.IsNullOrEmpty(publicKey)) return false;
        
            using var connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
            await connection.OpenAsync(_config["ROS:IP"], _config["ROS:PORTAL-Username"], _config["ROS:PORTAL-Password"]);
        
            var findCmd = connection.CreateCommandAndParameters(
                "/interface/wireguard/peers/print",
                "?public-key", publicKey);

            var rows = findCmd.ExecuteList().ToList();
            if(rows.Count == 0) return false;

            var id = rows[0].GetId();
            if(string.IsNullOrEmpty(id)) return false;
        
            var removeCmd = connection.CreateCommandAndParameters(
                "/interface/wireguard/peers/remove",
                ".id", id);
        
            removeCmd.ExecuteNonQuery();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing peer from router");
            return false;
        }
    }

    public async Task<bool> UpdateLastHandshakes()
    {
        try
        {
            using var connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
            await connection.OpenAsync(_config["ROS:IP"], _config["ROS:PORTAL-Username"], _config["ROS:PORTAL-Password"]);

            var cmd = connection.CreateCommandAndParameters("/interface/wireguard/peers/print");
            var rows = cmd.ExecuteList();

            var devicesToUpdate = new List<Device>();
            foreach (var r in rows)
            {
                var res = r.TryGetResponseField("comment", out var comment);
                if (!res || string.IsNullOrEmpty(comment)) continue;
                
                res = r.TryGetResponseField("last-handshake", out var lastHandshake);
                if (!res || string.IsNullOrEmpty(lastHandshake)) continue;
                
                if(!comment.Contains(':')) continue;
                var peerId = comment[(comment.IndexOf(':') + 1)..].Trim();
                
                if(lastHandshake.Equals("never", StringComparison.OrdinalIgnoreCase)) continue;
                lastHandshake = lastHandshake.Trim().ToLowerInvariant();
                var matches = Regex.Matches(lastHandshake, @"(?<val>\d+)\s*(?<unit>[smhdw])");
                if(matches.Count == 0) continue;
                
                long seconds = 0;
                foreach (Match match in matches)
                {
                    var v = long.Parse(match.Groups["val"].Value);
                    switch (match.Groups["unit"].Value)
                    {
                        case "s": seconds += v; break;
                        case "m": seconds += v * 60; break;
                        case "h": seconds += v * 60 * 60; break;
                        case "d": seconds += v * 86400; break;
                        case "w": seconds += v * 604800; break; 
                    }
                }
                var timespan = TimeSpan.FromSeconds(seconds);
                var lastHandshakeUtc = DateTime.UtcNow - timespan;

                var peer = await _context.DevicePeers
                    .Include(p => p.Device)
                    .FirstOrDefaultAsync(p => p.PeerId == peerId);
                if(peer?.Device == null) continue;

                var device = peer.Device;
                device.LastSeenUtc = lastHandshakeUtc;
                devicesToUpdate.Add(device);
            }
            
            if(devicesToUpdate.Count == 0) return true;
            
            _context.Devices.UpdateRange(devicesToUpdate);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last handshakes");
            return false;       
        }
    }
}