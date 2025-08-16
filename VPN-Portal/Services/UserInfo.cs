namespace VPN_Portal.Services;

public class UserInfo
{
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string? DisplayName { get; set; }
    public DateTime LastLogin { get; set; }
    public uint? MaxDeviceCount { get; set; }
    
    public bool IsSetup { get; set; } = false;
}