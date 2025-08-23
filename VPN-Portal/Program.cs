using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VPN_Portal.Areas.Admin.Services;
using VPN_Portal.Areas.Security.Services;
using VPN_Portal.Authentication;
using VPN_Portal.Authentication.UserClaims;
using VPN_Portal.Data;
using VPN_Portal.Middleware;
using VPN_Portal.Services;
using VPN_Portal.Services.Config;
using VPN_Portal.Services.DNS;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
        .EnableSensitiveDataLogging()
        .LogTo(Console.WriteLine, LogLevel.Information));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false; //TODO: Implement email confirmation
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<UserInfo>(sp => new UserInfo());
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, UserClaimsPrincipalFactory>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//Services:
builder.Services.AddScoped<DeviceService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<VpnManagementService>();
builder.Services.AddScoped<DnsManagementService>();
builder.Services.AddScoped<ReservationService>();
builder.Services.AddScoped<VpnService>();
builder.Services.AddScoped<VpnConfigService>();
builder.Services.AddScoped<PeerService>();
builder.Services.AddScoped<DownloadTokenService>();
builder.Services.AddTransient<KeyGenerationService>();
builder.Services.AddScoped<SecurityService>();
builder.Services.AddScoped<SecurityActionService>();
builder.Services.AddSingleton<SecurityCache>(sp =>
{
    var cache = new SecurityCache(sp, sp.GetRequiredService<IConfiguration>());
    cache.BuildCache().Wait();
    return cache;
});


builder.Services.AddRazorPages();

// Add Syncfusion services
var syncfusionLicenseKey = builder.Configuration.GetSection("SYNCFUSION-KEY").Value;
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(syncfusionLicenseKey);;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

await HostDefaults();
app.Run();


async Task HostDefaults()
{
    using var scope = app.Services.CreateScope();
    var sp = scope.ServiceProvider;
    var context = sp.GetRequiredService<ApplicationDbContext>();
    
    var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
    
    await context.Database.MigrateAsync();
    app.UseUserInfo();
    
    async Task ConfirmRoleSetup(string role)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    await ConfirmRoleSetup(SystemRoles.SystemAdmin);
    await ConfirmRoleSetup(SystemRoles.StandardUser);
    await ConfirmRoleSetup(SystemRoles.ReadOnlyUser);
    
    var adminUser = await userManager.FindByNameAsync("portaladmin");
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            Email = "jcraik23@gmail.com",
            UserName = "portaladmin",
            DisplayName = "Portal Admin",
            LastLogin = DateTime.Now,
            EmailConfirmed = true,
            TwoFactorEnabled = false
        };
        await userManager.CreateAsync(adminUser);
        await userManager.AddToRoleAsync(adminUser, SystemRoles.SystemAdmin);
        var p = "PortalAdmin@23";
        await userManager.AddPasswordAsync(adminUser, p);
    }
    else if (!await userManager.IsInRoleAsync(adminUser, SystemRoles.SystemAdmin))
    {
        await userManager.AddToRoleAsync(adminUser, SystemRoles.SystemAdmin);
    }
    
    //Load valid-users (assume DB is clean)
    var securityCache = sp.GetRequiredService<SecurityCache>();
    var userIds = await context.Users.Select(u => (ApplicationUser)u)
        .Where(u => !u.IsDeactivated).Select(u => u.Id).ToListAsync();
    await securityCache.UpdateValidUsersFile(userIds);
}