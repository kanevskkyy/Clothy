using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.AuthService.BLL.DTOs.Users
{
    public class UserReadDTO
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; } 
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; } 
        public string? PhotoUrl { get; set; } 
        public bool EmailVerified { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
