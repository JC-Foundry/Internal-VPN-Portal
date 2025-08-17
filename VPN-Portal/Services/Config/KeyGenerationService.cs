using NSec.Cryptography;
using VPN_Portal.Models.Devices;

namespace VPN_Portal.Services.Config;

public class KeyGenerationService
{
    private readonly FileService _privateKeyFiles;
    private readonly FileService _publicKeyFiles;
    
    public KeyGenerationService(IConfiguration config)
    {
        var basePath = config["BASE_PATH"];
        if(string.IsNullOrEmpty(basePath)) throw new Exception("BASE_PATH not set");
        
        _publicKeyFiles = new FileService(basePath, FileService.FileType.PublicKey);
        _privateKeyFiles = new FileService(basePath, FileService.FileType.PrivateKey);
    }
    
    private (string PublicKey, string PrivateKey) GenerateKeys()
    {
        var algorithm = KeyAgreementAlgorithm.X25519;
        using var key = new Key(algorithm, new KeyCreationParameters{ExportPolicy = KeyExportPolicies.AllowPlaintextExport});

        var privateKeyBytes = key.Export(KeyBlobFormat.RawPrivateKey);
        var privateKey = Convert.ToBase64String(privateKeyBytes);
        
        var publicKeyBytes = key.Export(KeyBlobFormat.RawPublicKey);
        var publicKey = Convert.ToBase64String(publicKeyBytes);
        
        return (publicKey, privateKey);
    }

    public async Task<(string PublicKey, string PrivateKey)> GetKeys(DevicePeer peer)
    {
        var (publicKey, privateKey) = GenerateKeys();
        
        await _publicKeyFiles.SaveFile($"{peer.PeerId}.txt", publicKey, peer.DeviceId);
        await _privateKeyFiles.SaveFile($"{peer.PeerId}.txt", privateKey, peer.DeviceId);
        
        return (publicKey, privateKey);
    }
}