using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.UserService.BLL.DTOs.RoleDTOs;

namespace Clothy.UserService.BLL.DTOs.UserDTOs
{
    public class UserReadDTO
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Email { get; set; }
        public List<RoleReadDTO>? Roles { get; set; }
    }
}
