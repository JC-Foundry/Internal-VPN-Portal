namespace VPN_Portal.Services;

public class FileService
{
    private readonly string _basePath;
    private readonly FileType _fileType;

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
        Directory.CreateDirectory(path);

        path = Path.Combine(path, fileName);
        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(content);
        await writer.FlushAsync();
        await stream.FlushAsync();
    }

    public string GetFile(string fileName, params string[] subFolders)
    {
        var subPath = GetSubPath();
        var path = Path.Combine(_basePath, subPath);
        path = subFolders.Aggregate(path, Path.Combine);
        path = Path.Combine(path, fileName);
        return File.ReadAllText(path);
    }
}