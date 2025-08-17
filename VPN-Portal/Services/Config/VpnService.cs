using tik4net;
using tik4net.Objects;
using VPN_Portal.Data;
using VPN_Portal.Helpers;
using VPN_Portal.Models.Devices;

namespace VPN_Portal.Services.Config;

public class VpnService
{
    private readonly IConfiguration _config;
    private readonly FileService _publicKeyFiles;

    public VpnService(IConfiguration config)
    {
        _config = config;
        var path = _config["BASE_PATH"];
        if(string.IsNullOrEmpty(path)) throw new Exception("BASE_PATH not set");
        
        _publicKeyFiles = new FileService(path, FileService.FileType.PublicKey);
    }
    
    public async Task<bool> AddPeerToRouter(DevicePeer peer)
    {
        var publicKey = _publicKeyFiles.GetFile($"{peer.PeerId}.txt", peer.DeviceId);
        if(string.IsNullOrEmpty(publicKey)) return false;
        
        var allowedIp = peer.AssignedIp();
        if(string.IsNullOrEmpty(allowedIp)) return false;
        var valid = IpAddressHelper.ValidateIpAddress(allowedIp);
        if(!valid) return false;
        
        using var connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
        await connection.OpenAsync(_config["ROUTER_IP"], _config["ROS_Username"], _config["ROS_Password"]);

        var cmd = connection.CreateCommandAndParameters("/interface/wireguard/peers/add",
            "interface", "wg-vpn",
            "public-key", publicKey,
            "allowed-address", $"{allowedIp}/32",
            "responder", "yes",
            "comment", $"Device '{peer.Device!.DeviceName}' ({peer.DeviceId}) peer: {peer.PeerId}",
            "persistent-keepalive", "25");
        
        cmd.ExecuteNonQuery();
        return true;
    }

    public async Task<bool> RemovePeerFromRouter(DevicePeer peer)
    {
        var publicKey = _publicKeyFiles.GetFile($"{peer.PeerId}.txt", peer.DeviceId);
        if(string.IsNullOrEmpty(publicKey)) return false;
        
        using var connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
        await connection.OpenAsync(_config["ROUTER_IP"], _config["ROS_Username"], _config["ROS_Password"]);
        
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
}