using System.Text;
using VPN_Portal.Data;
using VPN_Portal.Helpers;
using VPN_Portal.Models.Devices;

namespace VPN_Portal.Services.Config;

public class VpnConfigService
{
    private readonly FileService _configFiles;
    private readonly FileService _privateKeyFiles;
    
    public VpnConfigService(IConfiguration config)
    {
        var path = config["BASE_PATH"];
        if(string.IsNullOrEmpty(path)) throw new Exception("BASE_PATH not set");
        
        _configFiles = new FileService(path, FileService.FileType.Config);
        _privateKeyFiles = new FileService(path, FileService.FileType.PrivateKey);
    }
    
    private async Task<string?> BuildConfig(DevicePeer peer)
    {
        var privateKey = (await _privateKeyFiles.GetFileText($"{peer.PeerId}.txt", peer.DeviceId))?.Trim();
        if(string.IsNullOrEmpty(privateKey)) return "";
        try { _ = Convert.FromBase64String(privateKey); } catch { return null; }
        
        var allowedIp = peer.AssignedIp();
        if (string.IsNullOrEmpty(allowedIp)) return null;
        
        var valid = IpAddressHelper.ValidateIpAddress(allowedIp);
        if(!valid) return null;
        
        var sb = new StringBuilder(256);
        
        //Interface
        sb.AppendLine("[Interface]");
        sb.AppendLine($"PrivateKey = {privateKey}");
        sb.AppendLine($"Address = {allowedIp}/32");
        sb.AppendLine($"DNS = {peer.VpnServer!.ServerAddress}");
        sb.AppendLine();
        
        //Peer
        sb.AppendLine("[Peer]");
        sb.AppendLine($"PublicKey = {peer.VpnServer!.PublicKey}");
        sb.AppendLine($"AllowedIPs = {peer.VpnServer!.AddressCidr}");
        sb.AppendLine($"Endpoint = {peer.VpnServer!.EndpointHost}:{peer.VpnServer!.EndpointPort}");
        sb.AppendLine("PersistentKeepalive = 25");
        sb.AppendLine();

        var str = sb.ToString();
        var crlf = str.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
        if(!crlf.EndsWith("\r\n")) crlf += "\r\n";
        return crlf;
    }

    public async Task<bool> TryCreateConfig(DevicePeer peer)
    {
        var config = await BuildConfig(peer);
        if(string.IsNullOrEmpty(config)) return false;
        
        await _configFiles.SaveFile($"{peer.PeerId}.conf", config, peer.DeviceId);
        return true;
    }

    public async Task<(byte[]? ConfigBytes, string? ConfigText)> GetConfig(DevicePeer peer)
    {
        var configBytes = await _configFiles.GetFileBytes($"{peer.PeerId}.conf", peer.DeviceId);
        var configText = await _configFiles.GetFileText($"{peer.PeerId}.conf", peer.DeviceId);
        return (configBytes, configText);
    }

    public bool TryDeleteConfig(DevicePeer peer) => _configFiles.DeleteFile($"{peer.PeerId}.conf", peer.DeviceId); 
}
