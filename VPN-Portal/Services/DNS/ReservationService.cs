using VPN_Portal.Areas.Admin.Services;
using VPN_Portal.Data;
using VPN_Portal.Models.Devices;
using VPN_Portal.Models.VpnServers;

namespace VPN_Portal.Services.DNS;

public class ReservationService
{
    private readonly ApplicationDbContext _context;
    private readonly DnsManagementService _dnsManagementService;

    public ReservationService(ApplicationDbContext context,
    DnsManagementService dnsManagementService)
    {
        _context = context;
        _dnsManagementService = dnsManagementService;
    }

    private ushort? GetRandomHost(ushort min, ushort max, IEnumerable<ushort> taken)
    {
        var takenSet = new HashSet<ushort>(taken);
        var available = Enumerable.Range(min, max - min + 1)
            .Select(n => (ushort)n)
            .Where(n => !takenSet.Contains(n))
            .ToList();
        
        if(available.Count == 0) return null;
        
        var index = Random.Shared.Next(available.Count);
        return available[index];
    }

    public async Task<DnsReservation?> TryReserve(string deviceId, string peerId, string poolId)
    {
        //Get the pool:
        var pool = await _dnsManagementService.GetDnsPool(poolId);
        if(pool == null) return null;

        //Get a random host octet in the pool:
        var host = GetRandomHost(pool.MinHost, pool.MaxHost,
            pool.Reservations!.Where(r => r.IsActive)
                .Select(r => r.HostOctet).AsEnumerable());
        if(host == null) return null;

        //Create a reservation:
        var reservation = new DnsReservation
        {
            DeviceId = deviceId,
            DnsPoolId = poolId,
            HostOctet = (ushort)host,
            ReservedUtc = DateTime.UtcNow
        };
        await _context.DnsReservations.AddAsync(reservation);

        //Link to peer:
        var peerToReservation = new PeerToReservation
        {
            PeerId = peerId,
            ReservationId = reservation.ReservationId
        };
        await _context.PeerToReservations.AddAsync(peerToReservation);
        await _context.SaveChangesAsync();
        return reservation;
    }
}