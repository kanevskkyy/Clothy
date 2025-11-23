using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.UserService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clothy.UserService.DAL.DB
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(UserDbContext dbContext, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            await dbContext.Database.MigrateAsync();

            if (!await roleManager.RoleExistsAsync("Admin")) await roleManager.CreateAsync(new ApplicationRole { Name = "Admin" });
            if (!await roleManager.RoleExistsAsync("User")) await roleManager.CreateAsync(new ApplicationRole { Name = "User" });

            string clothyAdminEmail = "admin@clothy.com";
            string adminPassword = "Clothy127!";

            if (await userManager.FindByEmailAsync(clothyAdminEmail) == null)
            {
                ApplicationUser admin = new ApplicationUser
                {
                    FirstName = "Admin",
                    LastName = "Clothy",
                    UserName = clothyAdminEmail,
                    Email = clothyAdminEmail,
                    PhoneNumber = "+15059215743",
                    PhoneNumberConfirmed = true,
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(admin, adminPassword);
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
