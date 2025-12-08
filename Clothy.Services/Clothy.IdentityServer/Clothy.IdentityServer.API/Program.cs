using Clothy.IdentityServer.API.Data;
using Clothy.IdentityServer.API.Data.Entities;
using Clothy.IdentityServer.API.Data.Seed;
using Clothy.IdentityServer.API.Data.Service;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

string? connectionString = builder.Configuration.GetConnectionString("IdentityServerDb");
var migrationsAssembly = typeof(Program).Assembly.GetName().Name;

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddIdentityServer()
    .AddAspNetIdentity<ApplicationUser>()
    .AddConfigurationStore(options =>
    {
        options.ConfigureDbContext = b =>
            b.UseNpgsql(connectionString,
                sql => sql.MigrationsAssembly(migrationsAssembly));
    })
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = b =>
            b.UseNpgsql(connectionString,
                sql => sql.MigrationsAssembly(migrationsAssembly));
    })
    .AddDeveloperSigningCredential()
    .AddProfileService<ProfileService>();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        ApplicationDbContext appDb = services.GetRequiredService<ApplicationDbContext>();
        await appDb.Database.MigrateAsync();

        ConfigurationDbContext configDb = services.GetRequiredService<ConfigurationDbContext>();
        await configDb.Database.MigrateAsync();

        PersistedGrantDbContext grantDb = services.GetRequiredService<PersistedGrantDbContext>();
        await grantDb.Database.MigrateAsync();

        await SeedData.EnsureSeedDataAsync(services);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Migration Error: " + ex.Message);
        throw;
    }
}

app.UseStaticFiles();

app.UseRouting();
app.UseIdentityServer();
app.UseAuthorization();

app.MapDefaultControllerRoute();
app.MapRazorPages();

app.Run();