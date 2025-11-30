using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Clothy.Shared.Helpers.Exceptions;

namespace Clothy.Shared.Helpers.JWT
{
    public class UserClaimsExtractor : IUserClaimsExtractor
    {
        public Guid GetUserId(ClaimsPrincipal claimsPrincipal)
        {
            string? claimsUserId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? claimsPrincipal.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(claimsUserId)) throw new ValidationFailedException("Cannot find User ID in claims");
            
            return Guid.Parse(claimsUserId);
        }

        public string GetFirstName(ClaimsPrincipal claimsPrincipal)
        {
            string? firstName = claimsPrincipal.FindFirst(ClaimTypes.GivenName)?.Value;
            if (string.IsNullOrEmpty(firstName)) throw new ValidationFailedException("Cannot find User FirstName in claims");
            
            return firstName;
        }

        public string GetLastName(ClaimsPrincipal claimsPrincipal)
        {
            string? lastName = claimsPrincipal.FindFirst(ClaimTypes.Surname)?.Value;
            if (string.IsNullOrEmpty(lastName)) lastName = claimsPrincipal.FindFirst("family_name")?.Value;
            if (string.IsNullOrEmpty(lastName)) throw new ValidationFailedException("Cannot find User LastName in claims");

            return lastName;
        }

        public bool IsInRole(ClaimsPrincipal claimsPrincipal, string role)
        {
            return claimsPrincipal.IsInRole(role);
        }

        public string GetPhotoUrl(ClaimsPrincipal claimsPrincipal)
        {
            string? photoUrl = claimsPrincipal.FindFirst("photo_url")?.Value;
            return string.IsNullOrEmpty(photoUrl)
                ? "https://res.cloudinary.com/dkdljnfja/image/upload/v1763818143/Profile_Avatar_cfazhc.png"
                : photoUrl;
        }

        public string GetEmail(ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.FindFirst("email")?.Value 
                ?? claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value
                ?? throw new UnauthorizedAccessException("Email not found in token");
        }
    }
}
