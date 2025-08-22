using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VPN_Portal.Areas.Admin.Services;
using VPN_Portal.Authentication;
using VPN_Portal.Models.VpnServers;

namespace VPN_Portal.Areas.Admin.Pages.DNS
{
    [Authorize(Roles = SystemRoles.SystemAdmin)]
    public class IndexModel : PageModel
    {
        private readonly DnsManagementService _dnsManagementService;

        public IndexModel(DnsManagementService dnsManagementService)
        {
            _dnsManagementService = dnsManagementService;
        }

        public int TotalPools { get; set; }
        public int ActiveReservations { get; set; }
        public int AvailableIPs { get; set; }
        public int UtilizationRate { get; set; }
        public List<DnsPoolViewModel> TopDnsPools { get; set; } = new();
        public List<ReservationViewModel> RecentReservations { get; set; } = new();

        public async Task OnGetAsync()
        {
            var dnsPools = await _dnsManagementService.GetDnsPools();
            var dnsReservations = await _dnsManagementService.GetDnsReservations(true, 10);

            TotalPools = dnsPools.Count;
            ActiveReservations = dnsPools.Sum(p => p.Reservations!.Count(r => r.ReleasedUtc == null));
            
            var totalCapacity = dnsPools.Sum(p => p.MaxHost - p.MinHost + 1);
            AvailableIPs = totalCapacity - ActiveReservations;
            UtilizationRate = totalCapacity > 0 ? (int)((double)ActiveReservations / totalCapacity * 100) : 0;

            TopDnsPools = dnsPools
                .OrderByDescending(p => (p.Reservations!.Count(r => r.ReleasedUtc == null) / (double)(p.MaxHost - p.MinHost + 1)))
                .Take(5)
                .Select(pool => 
                {
                    var totalIps = pool.MaxHost - pool.MinHost + 1;
                    var usedIps = pool.Reservations!.Count(r => r.ReleasedUtc == null);
                    return new DnsPoolViewModel
                    {
                        Id = pool.DnsPoolId,
                        Name = pool.Name,
                        Subnet = GetSubnetString(pool),
                        TotalIPs = totalIps,
                        UsedIPs = usedIps,
                        AvailableIPs = totalIps - usedIps,
                        UsagePercentage = totalIps > 0 ? (int)((double)usedIps / totalIps * 100) : 0
                    };
                })
                .ToList();

            RecentReservations = dnsReservations
                .Select(r => new ReservationViewModel
                {
                    IPAddress = r.GetAddress(),
                    DeviceName = r.PeerToReservations.FirstOrDefault(ptr => ptr.IsActive)?.Peer?.Device?.DeviceName ?? "Unknown",
                    User = r.PeerToReservations.FirstOrDefault(ptr => ptr.IsActive)?.Peer?.Device?.User?.UserName ?? "Unknown",
                    PoolName = r.DnsPool?.Name ?? "Unknown",
                    ReservedAt = r.ReservedUtc,
                    IsReleased = !r.IsActive
                })
                .ToList();
        }

        private string GetNetworkString(NetworkFamily family)
        {
            return family switch
            {
                NetworkFamily.Net10 => "10.0.0.0/8",
                NetworkFamily.Net172 => "172.16.0.0/12",
                NetworkFamily.Net192 => "192.168.0.0/16",
                _ => "Unknown"
            };
        }

        private string GetSubnetString(DnsPool pool)
        {
            return pool.Family switch
            {
                NetworkFamily.Net10 => $"10.{pool.SecondOctet}.{pool.Subnet}.0/24",
                NetworkFamily.Net172 => $"172.{pool.SecondOctet}.{pool.Subnet}.0/24",
                NetworkFamily.Net192 => $"192.168.{pool.Subnet}.0/24",
                _ => "Unknown"
            };
        }

        public class DnsPoolViewModel
        {
            public string Id { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Subnet { get; set; } = string.Empty;
            public int TotalIPs { get; set; }
            public int UsedIPs { get; set; }
            public int AvailableIPs { get; set; }
            public int UsagePercentage { get; set; }
        }

        public class ReservationViewModel
        {
            public string IPAddress { get; set; } = string.Empty;
            public string User { get; set; } = string.Empty;
            public string DeviceName { get; set; } = string.Empty;
            public string PoolName { get; set; } = string.Empty;
            public DateTime ReservedAt { get; set; }
            public bool IsReleased { get; set; }
        }
    }
}