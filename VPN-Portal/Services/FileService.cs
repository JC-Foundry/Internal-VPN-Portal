using System.Text;

namespace VPN_Portal.Services;

public class FileService
{
    private readonly string _basePath;
    private readonly FileType _fileType;
    private readonly UTF8Encoding Utf8NoBom = new (encoderShouldEmitUTF8Identifier: false);

    public FileService(string basePath, FileType fileType)
    {
        _basePath = basePath;
        _fileType = fileType;
    }
    
    public enum FileType
    {
        Config,
        PrivateKey,
        PublicKey
    }

    private string GetSubPath()
        => _fileType switch
        {
            FileType.Config => "Device-Configs",
            FileType.PrivateKey => Path.Combine("Keys", "Private"),
            FileType.PublicKey => Path.Combine("Keys", "Public"),
            _ => throw new ArgumentOutOfRangeException()
        };

    public async Task SaveFile(string fileName, string content, params string[] subFolders)
    {
        var subPath = GetSubPath();
        
        var path = Path.Combine(_basePath, subPath);
        path = subFolders.Aggregate(path, Path.Combine);
        if(!Directory.Exists(path)) Directory.CreateDirectory(path);

        path = Path.Combine(path, fileName);
        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        await using var writer = new StreamWriter(stream, Utf8NoBom, bufferSize: 4096);
        writer.NewLine = "\r\n";
        await writer.WriteAsync(content);
        await writer.FlushAsync();
        await stream.FlushAsync();
    }

    public async Task<string?> GetFileText(string fileName, params string[] subFolders)
    {
        try
        {
            var subPath = GetSubPath();
            var path = Path.Combine(_basePath, subPath);
            path = subFolders.Aggregate(path, Path.Combine);
            path = Path.Combine(path, fileName);
            return await File.ReadAllTextAsync(path);
        }
        catch
        {
            return null;
        }
    }
    
    public async Task<byte[]?> GetFileBytes(string fileName, params string[] subFolders)
    {
        try
        {
            var subPath = GetSubPath();
            var path = Path.Combine(_basePath, subPath);
            path = subFolders.Aggregate(path, Path.Combine);
            path = Path.Combine(path, fileName);
            return await File.ReadAllBytesAsync(path);
        }
        catch
        {
            return null;
        }
    }

    public bool DeleteFile(string fileName, params string[] subFolders)
    {
        try
        {
            var subPath = GetSubPath();
            var path = Path.Combine(_basePath, subPath);
            path = subFolders.Aggregate(path, Path.Combine);
            path = Path.Combine(path, fileName);
            
            if (!File.Exists(path)) return false;
            File.Delete(path);
            return true;
        }
        catch
        {
            return false;
        }
    }
}