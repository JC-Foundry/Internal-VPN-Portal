using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VPN_Portal.Authentication;

namespace VPN_Portal.Areas.Admin.Pages.DNS
{
    [Authorize(Roles = SystemRoles.SystemAdmin)]
    public class IndexModel : PageModel
    {
        public int TotalPools { get; set; }
        public int ActiveReservations { get; set; }
        public int AvailableIPs { get; set; }
        public int UtilizationRate { get; set; }
        public List<DnsPoolViewModel> DnsPools { get; set; } = new();
        public List<ReservationViewModel> RecentReservations { get; set; } = new();

        public void OnGet()
        {
            // Dummy data for demonstration
            TotalPools = 3;
            ActiveReservations = 847;
            AvailableIPs = 2153;
            UtilizationRate = 39;

            // Dummy DNS pool data
            DnsPools = new List<DnsPoolViewModel>
            {
                new DnsPoolViewModel 
                { 
                    Name = "Primary Pool",
                    Network = "10.0.0.0/8",
                    Subnet = "10.1.0.0/16",
                    TotalIPs = 1024,
                    UsedIPs = 456,
                    AvailableIPs = 568,
                    UsagePercentage = 45
                },
                new DnsPoolViewModel 
                { 
                    Name = "Secondary Pool",
                    Network = "172.16.0.0/12",
                    Subnet = "172.16.1.0/24",
                    TotalIPs = 512,
                    UsedIPs = 318,
                    AvailableIPs = 194,
                    UsagePercentage = 62
                },
                new DnsPoolViewModel 
                { 
                    Name = "Guest Pool",
                    Network = "192.168.0.0/16",
                    Subnet = "192.168.100.0/24",
                    TotalIPs = 254,
                    UsedIPs = 73,
                    AvailableIPs = 181,
                    UsagePercentage = 29
                }
            };

            // Dummy recent reservations
            RecentReservations = new List<ReservationViewModel>
            {
                new ReservationViewModel 
                { 
                    IPAddress = "10.1.0.45",
                    DeviceName = "laptop-john",
                    PoolName = "Primary Pool",
                    ReservedAt = DateTime.Now.AddMinutes(-15)
                },
                new ReservationViewModel 
                { 
                    IPAddress = "172.16.1.123",
                    DeviceName = "mobile-sarah",
                    PoolName = "Secondary Pool",
                    ReservedAt = DateTime.Now.AddHours(-2)
                },
                new ReservationViewModel 
                { 
                    IPAddress = "192.168.100.67",
                    DeviceName = "tablet-mike",
                    PoolName = "Guest Pool",
                    ReservedAt = DateTime.Now.AddHours(-5)
                }
            };
        }

        public class DnsPoolViewModel
        {
            public string Name { get; set; } = string.Empty;
            public string Network { get; set; } = string.Empty;
            public string Subnet { get; set; } = string.Empty;
            public int TotalIPs { get; set; }
            public int UsedIPs { get; set; }
            public int AvailableIPs { get; set; }
            public int UsagePercentage { get; set; }
        }

        public class ReservationViewModel
        {
            public string IPAddress { get; set; } = string.Empty;
            public string DeviceName { get; set; } = string.Empty;
            public string PoolName { get; set; } = string.Empty;
            public DateTime ReservedAt { get; set; }
        }
    }
}