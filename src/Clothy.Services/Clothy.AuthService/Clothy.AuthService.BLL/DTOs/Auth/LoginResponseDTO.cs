using Clothy.AuthService.BLL.DTOs.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.AuthService.BLL.DTOs.Auth
{
    public class LoginResponseDTO
    {
        public UserReadDTO User { get; set; }
        public TokenResponseDTO Tokens { get; set; }
    }
}
