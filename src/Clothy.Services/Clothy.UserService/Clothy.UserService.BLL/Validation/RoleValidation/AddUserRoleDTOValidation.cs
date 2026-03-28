using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.UserService.BLL.DTOs.RoleDTOs;
using FluentValidation;

namespace Clothy.UserService.BLL.Validation.RoleValidation
{
    public class AddUserRoleDTOValidation : AbstractValidator<AddUserRoleDTO>
    {
        public AddUserRoleDTOValidation()
        {
            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required")
                .Must(role => AllowedRoles.Contains(role))
                .WithMessage("Role is not valid");
        }

        private static string[] AllowedRoles = { "Admin", "User" };
    }
}
