using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VPN_Portal.Models;
using VPN_Portal.Models.Devices;
using VPN_Portal.Models.VpnServers;
using VPN_Portal.Services;

namespace VPN_Portal.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Device> Devices { get; set; }
    public DbSet<DevicePeer> DevicePeers { get; set; }
    public DbSet<PeerToReservation> PeerToReservations { get; set; }
    public DbSet<AllowedIp> AllowedIps { get; set; }
    
    public DbSet<VpnServer> VpnServers { get; set; }
    public DbSet<DnsPool> DnsPools { get; set; }
    public DbSet<DnsReservation> DnsReservations { get; set; }
    
    public DbSet<DownloadToken> DownloadTokens { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Fix the multiple cascade paths issue for SQL Server
        modelBuilder.Entity<PeerToReservation>()
            .HasOne(p => p.DnsReservation)
            .WithMany()
            .HasForeignKey(p => p.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<PeerToReservation>()
            .HasOne(p => p.Peer)
            .WithMany()
            .HasForeignKey(p => p.PeerId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // Fix any other cascade issues
        modelBuilder.Entity<DnsReservation>()
            .HasOne(d => d.Device)
            .WithMany()
            .HasForeignKey(d => d.DeviceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}