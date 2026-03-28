using Clothy.IdentityServer.API.Data.Entities;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Clothy.IdentityServer.API.Data.Service
{
    public class ProfileService : IProfileService
    {
        private UserManager<ApplicationUser> userManager;

        public ProfileService(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = await userManager.GetUserAsync(context.Subject);

            if (user != null)
            {
                List<Claim> claims = new List<Claim>
                {
                    new Claim("sub", user.Id.ToString()),
                    new Claim("name", user.UserName ?? ""),
                    new Claim("email", user.Email ?? "")
                };

                if (!string.IsNullOrEmpty(user.FirstName)) claims.Add(new Claim("given_name", user.FirstName));
                if (!string.IsNullOrEmpty(user.LastName)) claims.Add(new Claim("family_name", user.LastName));
                if (!string.IsNullOrEmpty(user.PhoneNumber)) claims.Add(new Claim("phone_number", user.PhoneNumber));
                if (!string.IsNullOrEmpty(user.PhotoUrl)) claims.Add(new Claim("photo_url", user.PhotoUrl));

                var roles = await userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    claims.Add(new Claim("role", role));
                }

                context.IssuedClaims.AddRange(claims);
            }
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            ApplicationUser? user = await userManager.GetUserAsync(context.Subject);
            context.IsActive = user != null;
        }
    }
}
