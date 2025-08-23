using Newtonsoft.Json;
using VPN_Portal.Services;

namespace VPN_Portal.Areas.Security.Services;

public class SecurityCache
{
    private readonly IServiceProvider _serviceProvider;
    private readonly FileService _fileService;
    
    private readonly Dictionary<string, string> _userCache = new();

    public SecurityCache(IServiceProvider serviceProvider,
        IConfiguration config)
    {
        _serviceProvider = serviceProvider;
        
        var path = config["BASE_PATH"];
        if(string.IsNullOrEmpty(path)) throw new Exception("BASE_PATH not set in config.");
        
        _fileService = new FileService(path, FileService.FileType.Security);
    }
    
    private const string ValidUsersFile = "valid-users.txt";
    

    private async Task BuildUserCache()
    {
        var usersJson = await _fileService.GetFileText(ValidUsersFile);
        if(string.IsNullOrEmpty(usersJson)) return;
        
        var users = JsonConvert.DeserializeObject<string[]>(usersJson);
        if(users == null) return;

        foreach (var user in users)
        {
            _userCache.TryAdd(user, user);
        }
    }
    
    public async Task BuildCache()
    {
        await BuildUserCache();
    }
    
    public async Task<bool> ValidateUser(string userId)
    {
        if(_userCache.ContainsKey(userId)) return true;
        
        var usersJson = await _fileService.GetFileText(ValidUsersFile);
        if(string.IsNullOrEmpty(usersJson)) return false;
        
        var users = JsonConvert.DeserializeObject<string[]>(usersJson);
        if(users == null) return false; 
        
        if(!users.Contains(userId)) return false;
        
        _userCache.TryAdd(userId, userId);
        return true;
    }

    public async Task UpdateValidUsersFile(string userId)
    {
        var usersJson = await _fileService.GetFileText(ValidUsersFile);
        if(string.IsNullOrEmpty(usersJson)) usersJson = "[]";
        
        _fileService.DeleteFile(ValidUsersFile);
        
        var users = JsonConvert.DeserializeObject<List<string>>(usersJson) ?? [];
        users.Add(userId);
        usersJson = JsonConvert.SerializeObject(users);
        await _fileService.SaveFile(ValidUsersFile, usersJson);
    }
    
    public async Task UpdateValidUsersFile(List<string> userIds)
    {
        var usersJson = await _fileService.GetFileText(ValidUsersFile);
        if(string.IsNullOrEmpty(usersJson)) usersJson = "[]";
        
        _fileService.DeleteFile(ValidUsersFile);
        
        var users = JsonConvert.DeserializeObject<List<string>>(usersJson) ?? [];
        foreach (var uid in userIds.Where(uid => !users.Contains(uid)))
        {
            users.Add(uid);
        }

        foreach (var uid in users.ToList().Where(uid => !userIds.Contains(uid)))
        {
            users.Remove(uid);
        }
        
        usersJson = JsonConvert.SerializeObject(users);
        await _fileService.SaveFile(ValidUsersFile, usersJson);
    }
}