using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Clothy.AuthService.BLL.DTOs.Auth
{
    public class ResendVerificationEmailDTO
    {
        public string? Email { get; set; }
    }
}