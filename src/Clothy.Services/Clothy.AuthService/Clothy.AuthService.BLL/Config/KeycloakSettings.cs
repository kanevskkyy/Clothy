using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.AuthService.BLL.Config
{
    public class KeycloakSettings
    {
        public string? Url { get; set; }
        public string? Realm { get; set; } 
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
    }
}
