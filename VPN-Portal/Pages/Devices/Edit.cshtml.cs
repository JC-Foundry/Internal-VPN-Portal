using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using VPN_Portal.Extensions;
using VPN_Portal.Models.Devices;
using VPN_Portal.Services;

namespace VPN_Portal.Pages.Devices;

[Authorize]
public class Edit : PageModel
{
    private readonly DeviceService _deviceService;
    private readonly UserInfo _userInfo;
    public bool IsAdding { get; set; } = true;

    public Edit(DeviceService deviceService,
        UserInfo userInfo)
    {
        _deviceService = deviceService;
        _userInfo = userInfo;
    }
    
    public class DeviceInputModel
    {
        [Required]
        [DisplayName("Device Name")]
        public string DeviceName { get; set; }
        [Required]
        [DisplayName("Device Type")]
        public DeviceType DeviceType { get; set; }

        public DeviceInputModel()
        {
        }

        public DeviceInputModel(Device device)
        {
            DeviceName = device.DeviceName;
            DeviceType = device.DeviceType;
        }
        
        public void Fill(Device device, string userId)
        {
            device.DeviceName = DeviceName;
            device.DeviceType = DeviceType;
            device.UserId = userId;
        }
    }
    
    [BindProperty]
    public DeviceInputModel Input { get; set; }
    public List<SelectListItem> DeviceTypes { get; set; }

    public void SetupAdding(string? deviceId)
    {
        IsAdding = deviceId == null;
    }

    public void SetupPage()
    {
        DeviceTypes = DeviceType.Desktop.GetAllOptions()
            .Select(dt => new SelectListItem(dt.Name, dt.Value.ToString()))
            .ToList();
    }

    public async Task<IActionResult> OnGetAsync(string? id)
    {
        SetupAdding(id);
        SetupPage();
        
        if (IsAdding)
        {
            Input = new DeviceInputModel();
            return Page();
        }
        
        var device = await _deviceService.GetDevice(id!);
        if (device == null) return NotFound();
        
        Input = new DeviceInputModel(device);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? id)
    {
        SetupAdding(id);
        SetupPage();
        if (!ModelState.IsValid) return Page();

        bool res;
        if (IsAdding)
        {
            var device = new Device();
            Input.Fill(device, _userInfo.UserId);
            res = await _deviceService.TryAddDevice(device, ModelState);
        }
        else
        {
            var device = await _deviceService.GetDevice(id!);
            if (device == null) return NotFound();
            
            Input.Fill(device, _userInfo.UserId);
            res = await _deviceService.TryUpdateDevice(device, ModelState);
        }
        
        return res ? RedirectToPage("/Index") : Page();
    }
}