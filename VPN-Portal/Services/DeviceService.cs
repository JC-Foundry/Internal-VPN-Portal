using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using VPN_Portal.Data;
using Device = VPN_Portal.Models.Devices.Device;

namespace VPN_Portal.Services;

public class DeviceService
{
    private readonly ApplicationDbContext _context;
    private readonly UserInfo _userInfo;


    public DeviceService(ApplicationDbContext context,
        UserInfo userInfo)
    {
        _context = context;
        _userInfo = userInfo;
    }
    

    public async Task<List<Device>> GetDevices(bool asNoTracking = true)
    {
        var query = _context.Devices
            .Include(d => d.Peers)!
            .ThenInclude(p => p.VpnServer)
            .Where(d => d.UserId == _userInfo.UserId && !d.IsRevoked);
        
        if (asNoTracking) query = query.AsNoTracking();
        return await query.OrderBy(d => d.DeviceName).ToListAsync();
    }
    
    public async Task<List<Device>> GetDevicesForUser(string userId, bool includeRevoked = false, bool asNoTracking = true)
    {
        var query = _context.Devices
            .Include(d => d.Peers)!
            .ThenInclude(p => p.VpnServer)
            .Where(d => d.UserId == userId);
        
        if (!includeRevoked)
            query = query.Where(d => !d.IsRevoked);
        
        if (asNoTracking) query = query.AsNoTracking();
        return await query.OrderBy(d => d.DeviceName).ToListAsync();
    }
    
    public async Task<Device?> GetDevice(string deviceId, bool asNoTracking = true)
    {
        IQueryable<Device> query = _context.Devices;
        if (asNoTracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(d => d.DeviceId == deviceId 
                                                    && d.UserId == _userInfo.UserId
                                                    && !d.IsRevoked);
    }

    
    private async Task ValidateDevice(bool adding, Device device, ModelStateDictionary modelState)
    {
        var existingDevice = await _context.Devices.FirstOrDefaultAsync(d => d.UserId == device.UserId 
                                                                             && d.DeviceName == device.DeviceName
                                                                             && d.DeviceId != device.DeviceId);
        if (existingDevice != null)
        {
            modelState.AddModelError($"Input.{nameof(device.DeviceName)}", "A device with this name already exists.");
        }

        if (adding)
        {
            var userDeviceCount = await _context.Devices.CountAsync(d => d.UserId == device.UserId && !d.IsRevoked);
            var maxCount = _userInfo.MaxDeviceCount ?? 0;
            if (userDeviceCount >= maxCount)
            {
                modelState.AddModelError($"Input", "You have reached the maximum number of devices.");
            }
        }
    }

    public async Task<bool> TryAddDevice(Device device, ModelStateDictionary modelState)
    {
        await ValidateDevice(true, device, modelState);
        if (!modelState.IsValid) return false;
        
        _context.Devices.Add(device);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TryUpdateDevice(Device device, ModelStateDictionary modelState)
    {
        await ValidateDevice(false, device, modelState);
        if (!modelState.IsValid) return false;
        
        _context.Devices.Update(device);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokeDevice(string deviceId)
    {
        var device = await GetDevice(deviceId);
        if (device == null) return false;
        
        device.RevokeDevice();
        _context.Devices.Update(device);
        await _context.SaveChangesAsync();
        return true;
    }
}