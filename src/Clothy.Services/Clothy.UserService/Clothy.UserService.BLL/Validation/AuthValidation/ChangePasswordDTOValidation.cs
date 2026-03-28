using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.UserService.BLL.DTOs.AuthDTOs;
using FluentValidation;

namespace Clothy.UserService.BLL.Validation.AuthValidation
{
    public class ChangePasswordDTOValidation : AbstractValidator<ChangePasswordDTO>
    {
        public ChangePasswordDTOValidation()
        {
            RuleFor(p => p.OldPassword)
                .NotEmpty().WithMessage("Old password cannot be empty");

            RuleFor(p => p.NewPassword)
                .NotEmpty().WithMessage("New password cannot be empty")
                .MinimumLength(6).WithMessage("New password must be at least 6 characters")
                .Matches("[A-Z]").WithMessage("New password must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("New password must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("New password must contain at least one number");

            RuleFor(p => p.ConfirmPassword)
                .Equal(p => p.NewPassword).WithMessage("Confirm password must match new password");
        }
    }
}
