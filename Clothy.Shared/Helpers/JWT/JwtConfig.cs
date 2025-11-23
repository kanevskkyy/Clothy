using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.Shared.Helpers.JWT
{
    public class JwtConfig
    {
        public string? Key { get; set; }
        public string? Audience { get; set; }
        public string? Issuer { get; set; }
        public int AccessTokenDurationMinutes { get; set; } = 15;
        public int RefreshTokenDurationDays { get; set; } = 20;
    }
}
