using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.AuthService.BLL.DTOs.Auth
{
    public class ForgotPasswordDTO
    {
        public string? Email { get; set; }
    }
}
