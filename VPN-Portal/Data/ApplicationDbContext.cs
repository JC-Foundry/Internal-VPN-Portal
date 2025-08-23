using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VPN_Portal.Models;
using VPN_Portal.Models.Devices;
using VPN_Portal.Models.Security;
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
    
    public DbSet<VpnServer> VpnServers { get; set; }
    public DbSet<VpnServerUser> VpnServerUsers { get; set; }
    public DbSet<DnsPool> DnsPools { get; set; }
    public DbSet<DnsReservation> DnsReservations { get; set; }
    
    public DbSet<DownloadToken> DownloadTokens { get; set; }
    public DbSet<DownloadEvent> DownloadEvents { get; set; }

    #region Security

    public DbSet<SecurityEvent> SecurityEvents { get; set; }
    public DbSet<AddRouterPeerEvent> AddRouterPeerEvents { get; set; }
    public DbSet<CreateUserEvent> CreateUserEvents { get; set; }
    public DbSet<InvalidLoginAttemptEvent> InvalidLoginAttemptEvents { get; set; }
    public DbSet<InvalidTokenEvent> InvalidTokenEvents { get; set; }
    public DbSet<UnauthorisedVpnPeerEvent> UnauthorisedVpnPeerEvents { get; set; }
    public DbSet<UnauthorisedVpnTokenEvent> UnauthorisedVpnTokenEvents { get; set; }
    public DbSet<PeerAbuseEvent> PeerAbuseEvents { get; set; }
    public DbSet<SecurityAction> SecurityActions { get; set; }
 
    #endregion

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
        
        // Fix cascade issues for DownloadTokens and DownloadEvents
        modelBuilder.Entity<DownloadToken>()
            .HasOne(dt => dt.User)
            .WithMany()
            .HasForeignKey(dt => dt.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<DownloadToken>()
            .HasOne(dt => dt.Peer)
            .WithMany()
            .HasForeignKey(dt => dt.PeerId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<DownloadEvent>()
            .HasOne(de => de.User)
            .WithMany()
            .HasForeignKey(de => de.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<DownloadEvent>()
            .HasOne(de => de.Peer)
            .WithMany()
            .HasForeignKey(de => de.PeerId)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<DownloadEvent>()
            .HasOne(de => de.Token)
            .WithMany()
            .HasForeignKey(de => de.TokenId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Fix cascade issues for UnauthorisedVpnPeerEvent
        modelBuilder.Entity<UnauthorisedVpnPeerEvent>()
            .HasOne(u => u.VpnServer)
            .WithMany()
            .HasForeignKey(u => u.VpnServerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<UnauthorisedVpnPeerEvent>()
            .HasOne(u => u.Peer)
            .WithMany()
            .HasForeignKey(u => u.PeerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Fix cascade issues for UnauthorisedVpnTokenEvent
        modelBuilder.Entity<UnauthorisedVpnTokenEvent>()
            .HasOne(u => u.VpnServer)
            .WithMany()
            .HasForeignKey(u => u.VpnServerId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<UnauthorisedVpnTokenEvent>()
            .HasOne(u => u.Peer)
            .WithMany()
            .HasForeignKey(u => u.PeerId)
            .OnDelete(DeleteBehavior.Restrict);
        
    }
}