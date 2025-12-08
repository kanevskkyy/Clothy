using Duende.IdentityServer.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Duende.IdentityServer.EntityFramework.Mappers;
using Clothy.IdentityServer.API.Data.Entities;


namespace Clothy.IdentityServer.API.Data.Seed
{
    public static class SeedData
    {
        public static async Task EnsureSeedDataAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;

            ApplicationDbContext appDb = sp.GetRequiredService<ApplicationDbContext>();
            ConfigurationDbContext configDb = sp.GetRequiredService<ConfigurationDbContext>();
            PersistedGrantDbContext grantDb = sp.GetRequiredService<PersistedGrantDbContext>();

            if (!configDb.Clients.Any())
            {
                foreach (var client in Config.Clients)
                {
                    configDb.Clients.Add(client.ToEntity());
                }
                await configDb.SaveChangesAsync();
            }

            if (!configDb.IdentityResources.Any())
            {
                foreach (var resource in Config.IdentityResources)
                {
                    configDb.IdentityResources.Add(resource.ToEntity());
                }
                await configDb.SaveChangesAsync();
            }

            if (!configDb.ApiScopes.Any())
            {
                foreach (var scopeEntity in Config.ApiScopes)
                {
                    configDb.ApiScopes.Add(scopeEntity.ToEntity());
                }
                await configDb.SaveChangesAsync();
            }

            RoleManager<IdentityRole> roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roles = { "Admin", "Manager", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            UserManager<ApplicationUser> userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

            string adminEmail = "admin@clothy.com";
            ApplicationUser? admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = "Admin",
                    Email = adminEmail,
                    FirstName = "Super",
                    LastName = "Admin",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(admin, "Admin@1234");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            string managerEmail = "manager@clothy.com";
            ApplicationUser? manager = await userManager.FindByEmailAsync(managerEmail);

            if (manager == null)
            {
                manager = new ApplicationUser
                {
                    UserName = "Manager",
                    Email = managerEmail,
                    FirstName = "Main",
                    LastName = "Manager",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(manager, "Manager@1234");
                await userManager.AddToRoleAsync(manager, "Manager");
            }

            string userEmail = "user@clothy.com";
            ApplicationUser? user = await userManager.FindByEmailAsync(userEmail);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = "User",
                    Email = userEmail,
                    FirstName = "Regular",
                    LastName = "User",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(user, "User@1234");
                await userManager.AddToRoleAsync(user, "User");
            }
        }
    }
}
