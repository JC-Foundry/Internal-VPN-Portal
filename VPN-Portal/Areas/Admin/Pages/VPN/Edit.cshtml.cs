using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using VPN_Portal.Areas.Admin.Services;
using VPN_Portal.Authentication;
using VPN_Portal.Models.VpnServers;

namespace VPN_Portal.Areas.Admin.Pages.VPN;

[Authorize(Roles = SystemRoles.SystemAdmin)]
public class EditModel : PageModel
{
    private readonly VpnManagementService _vpnManagementService;
    private readonly DnsManagementService _dnsManagementService;
    private readonly AdminService _adminService;

    public bool IsAdding { get; set; } = true;

    public EditModel(VpnManagementService vpnManagementService,
        DnsManagementService dnsManagementService,
        AdminService adminService)
    {
        _vpnManagementService = vpnManagementService;
        _dnsManagementService = dnsManagementService;
        _adminService = adminService;
    }
    
    public class VpnServerInputModel
    {
        [Required]
        [DisplayName("Server Name")]
        [StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        [DisplayName("Endpoint Host")]
        [StringLength(255)]
        public string EndpointHost { get; set; }
        
        [Required]
        [DisplayName("Endpoint Port")]
        [Range(1, 65535)]
        public uint EndpointPort { get; set; }

        [Required]
        [DisplayName("Interface Name")]
        [StringLength(150)]
        public string InterfaceName { get; set; }
        
        [Required]
        [DisplayName("Public Key")]
        [StringLength(500)]
        public string PublicKey { get; set; }
        
        [Required]
        [DisplayName("Address CIDR")]
        [StringLength(50)]
        public string AddressCidr { get; set; }
        
        [Required]
        [DisplayName("Server Address")]
        [StringLength(50)]
        public string ServerAddress { get; set; }
        
        [Required]
        [DisplayName("DNS Pool")]
        public string DnsPoolId { get; set; }

        public VpnServerInputModel()
        {
        }

        public VpnServerInputModel(VpnServer vpnServer)
        {
            Name = vpnServer.Name;
            EndpointHost = vpnServer.EndpointHost;
            EndpointPort = vpnServer.EndpointPort;
            InterfaceName = vpnServer.InterfaceName;
            PublicKey = vpnServer.PublicKey;
            AddressCidr = vpnServer.AddressCidr;
            ServerAddress = vpnServer.ServerAddress;
            DnsPoolId = vpnServer.DnsPoolId;
        }
        
        public void Fill(VpnServer vpnServer)
        {
            vpnServer.Name = Name;
            vpnServer.EndpointHost = EndpointHost;
            vpnServer.EndpointPort = EndpointPort;
            vpnServer.InterfaceName = InterfaceName;
            vpnServer.PublicKey = PublicKey;
            vpnServer.AddressCidr = AddressCidr;
            vpnServer.ServerAddress = ServerAddress;
            vpnServer.DnsPoolId = DnsPoolId;
        }
    }
    
    [BindProperty]
    public VpnServerInputModel Input { get; set; }
    [BindProperty]
    public List<string> UserIds { get; set; }
    public List<(string UserName, string UserId)> Users { get; set; }
    
    public List<SelectListItem> DnsPools { get; set; }

    public void SetupAdding(string? vpnServerId)
    {
        IsAdding = vpnServerId == null;
    }

    public async Task SetupPage()
    {
        DnsPools = (await _dnsManagementService.GetDnsPools()).Select(p => new SelectListItem
        {
            Text =
                $"{p.Name} - {p.GetRange()}",
            Value = p.DnsPoolId
        }).ToList();

        var users = await _adminService.GetUsers(true);
        Users = users.Where(u => u.UserName != null)
            .Select(u => (u.UserName!, u.Id)).ToList();
    }

    public async Task<IActionResult> OnGetAsync(string? id)
    {
        SetupAdding(id);
        await SetupPage();
        
        if (IsAdding)
        {
            Input = new VpnServerInputModel();
            return Page();
        }
        
        var vpnServer = await _vpnManagementService.GetVpnServer(id!, includePool: false);
        if (vpnServer == null) return NotFound();
        
        UserIds = await _vpnManagementService.GetVpnServerUserIds(vpnServer.VpnServerId);
        Input = new VpnServerInputModel(vpnServer);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? id)
    {
        SetupAdding(id);
        await SetupPage();
        
        if (!ModelState.IsValid) return Page();

        bool res;
        if (IsAdding)
        {
            var vpnServer = new VpnServer();
            Input.Fill(vpnServer);
            res = await _vpnManagementService.TryAddVpnServer(vpnServer, UserIds, ModelState);
        }
        else
        {
            var vpnServer = await _vpnManagementService.GetVpnServer(id!, includePool: false);
            if (vpnServer == null) return NotFound();
            
            Input.Fill(vpnServer);
            res = await _vpnManagementService.TryUpdateVpnServer(vpnServer, UserIds, ModelState);
        }
        
        return res ? RedirectToPage("/VPN/Index", new { area = "Admin" }) : Page();
    }
}