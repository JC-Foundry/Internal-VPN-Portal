using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using VPN_Portal.Data;
using VPN_Portal.Models.VpnServers;

namespace VPN_Portal.Areas.Admin.Services;

public class DnsManagementService
{
    private readonly ApplicationDbContext _context;

    public DnsManagementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DnsPool>> GetDnsPools(bool asNoTracking = true)
    {
        var query = _context.DnsPools.Include(p => p.Reservations)
            .Where(p => p.Enabled);
        if(asNoTracking) query = query.AsNoTracking();
        return await query.OrderBy(p => p.Name).ToListAsync();
    }
    
    public async Task<DnsPool?> GetDnsPool(string dnsPoolId, bool asNoTracking = true)
    {
        var query = _context.DnsPools.Include(p => p.Reservations)
            .Where(p => p.Enabled);
        if(asNoTracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(p => p.DnsPoolId == dnsPoolId);
    }

    private async Task ValidateDnsPool(bool adding, DnsPool dnsPool, ushort? secondSubnet, ModelStateDictionary modelState)
    {
        var existing = await _context.DnsPools.AnyAsync(p => p.Name == dnsPool.Name 
                                                             && p.DnsPoolId != dnsPool.DnsPoolId);
        if (existing)
        {
            modelState.AddModelError($"Input.{nameof(dnsPool.Name)}", "A DNS pool with this name already exists.");
        }
        
        if (dnsPool.MaxHost <= dnsPool.MinHost)
        {
            modelState.AddModelError($"Input.{nameof(dnsPool.MaxHost)}", "The maximum host count must be greater than the minimum host count.");
        }

        if (dnsPool.Family != NetworkFamily.Net192)
        {
            var res = dnsPool.SetSecondOctet(secondSubnet ?? 0);
            if (!res)
            {
                modelState.AddModelError($"Input.{nameof(dnsPool.SecondOctet)}", "Invalid second octet.");
            }
        }
        else
        {
            dnsPool.SetSecondOctet(0);
        }
    }

    public async Task<bool> TryAddDnsPool(DnsPool dnsPool, ushort? secondSubnet, ModelStateDictionary modelState)
    {
        await ValidateDnsPool(true, dnsPool, secondSubnet, modelState);
        if (!modelState.IsValid) return false;
        
        _context.DnsPools.Add(dnsPool);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> TryUpdateDnsPool(DnsPool dnsPool, ushort? secondSubnet, ModelStateDictionary modelState)
    {
        await ValidateDnsPool(false, dnsPool, secondSubnet, modelState);
        if (!modelState.IsValid) return false;
        
        _context.DnsPools.Update(dnsPool);
        await _context.SaveChangesAsync();
        return true;
    }
}