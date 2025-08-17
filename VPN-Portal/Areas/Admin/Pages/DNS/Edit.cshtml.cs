using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using VPN_Portal.Areas.Admin.Services;
using VPN_Portal.Authentication;
using VPN_Portal.Extensions;
using VPN_Portal.Models.VpnServers;

namespace VPN_Portal.Areas.Admin.Pages.DNS;

[Authorize(Roles = SystemRoles.SystemAdmin)]
public class EditModel : PageModel
{
    private readonly DnsManagementService _dnsManagementService;
    
    public bool IsAdding { get; set; } = true;

    public EditModel(DnsManagementService dnsManagementService)
    {
        _dnsManagementService = dnsManagementService;
    }
    
    public class DnsPoolInputModel
    {
        [Required]
        [DisplayName("Pool Name")]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        [DisplayName("Network Family")]
        public NetworkFamily Family { get; set; }
        
        [DisplayName("Second Octet")]
        [Range(0, 255)]
        public ushort? SecondOctet { get; set; }
        
        [Required]
        [DisplayName("Subnet")]
        [Range(0, 255)]
        public ushort Subnet { get; set; }
        
        [Required]
        [DisplayName("Minimum Host")]
        [Range(2, 254)]
        public ushort MinHost { get; set; }
        
        [Required]
        [DisplayName("Maximum Host")]
        [Range(2, 254)]
        public ushort MaxHost { get; set; }
        
        [DisplayName("Enabled")]
        public bool Enabled { get; set; } = true;

        public DnsPoolInputModel()
        {
        }

        public DnsPoolInputModel(DnsPool dnsPool)
        {
            Name = dnsPool.Name;
            Family = dnsPool.Family;
            SecondOctet = dnsPool.SecondOctet;
            Subnet = dnsPool.Subnet;
            MinHost = dnsPool.MinHost;
            MaxHost = dnsPool.MaxHost;
            Enabled = dnsPool.Enabled;
        }
        
        public void Fill(DnsPool dnsPool)
        {
            dnsPool.Name = Name;
            dnsPool.Family = Family;
            dnsPool.Subnet = Subnet;
            dnsPool.MinHost = MinHost;
            dnsPool.MaxHost = MaxHost;
            dnsPool.Enabled = Enabled;
        }
    }
    
    [BindProperty]
    public DnsPoolInputModel Input { get; set; }
    
    public List<SelectListItem> NetworkFamilies { get; set; }

    public void SetupAdding(string? dnsPoolId)
    {
        IsAdding = dnsPoolId == null;
    }

    public void SetupPage()
    {
        NetworkFamilies = NetworkFamily.Net10.GetAllOptions()
            .Select(nf => new SelectListItem(nf.Name, nf.Value.ToString()))
            .ToList();
    }

    public async Task<IActionResult> OnGetAsync(string? id)
    {
        SetupAdding(id);
        SetupPage();
        
        if (IsAdding)
        {
            Input = new DnsPoolInputModel();
            return Page();
        }
        
        var dnsPool = await _dnsManagementService.GetDnsPool(id!);
        if (dnsPool == null) return NotFound();
        
        Input = new DnsPoolInputModel(dnsPool);
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
            var dnsPool = new DnsPool();
            Input.Fill(dnsPool);
            res = await _dnsManagementService.TryAddDnsPool(dnsPool, Input.SecondOctet, ModelState);
        }
        else
        {
            var dnsPool = await _dnsManagementService.GetDnsPool(id!);
            if (dnsPool == null) return NotFound();
            
            Input.Fill(dnsPool);
            res = await _dnsManagementService.TryUpdateDnsPool(dnsPool, Input.SecondOctet, ModelState);
        }
        
        return res ? RedirectToPage("/DNS/Pools", new { area = "Admin" }) : Page();
    }
}