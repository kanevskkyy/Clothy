using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Shared.Helpers.JWT
{
    public interface IUserClaimsExtractor
    {
        Guid GetUserId(ClaimsPrincipal claimsPrincipal);
        string GetFirstName(ClaimsPrincipal claimsPrincipal);
        string GetLastName(ClaimsPrincipal claimsPrincipal);
        string GetPhotoUrl(ClaimsPrincipal claimsPrincipal);
        bool IsInRole(ClaimsPrincipal claimsPrincipal, string role);
    }
}
