using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.UserService.BLL.DTOs.AuthDTOs;
using FluentValidation;

namespace Clothy.UserService.BLL.Validation.AuthValidation
{
    public class RegisterDTOValidation : AbstractValidator<RegisterDTO>
    {
        public RegisterDTOValidation()
        {
            RuleFor(p => p.Email)
                .EmailAddress().WithMessage("Email should be valid")
                .NotEmpty().WithMessage("Email is required")
                .MaximumLength(256).WithMessage("Email should be less than 256 symbols");

            RuleFor(p => p.FirstName)
                .NotEmpty().WithMessage("FirstName is required")
                .MaximumLength(50).WithMessage("FirstName should be less than 50 symbols");

            RuleFor(p => p.LastName)
                .NotEmpty().WithMessage("LastName is required")
                .MaximumLength(50).WithMessage("LastName should be less than 50 symbols");

            RuleFor(p => p.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^(\+380|0)\d{9}$").WithMessage("Phone number must be a valid Ukrainian number");

            RuleFor(p => p.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
                .Matches("[0-9]").WithMessage("Password must contain at least one number");

            RuleFor(p => p.ConfirmPassword)
                .Equal(p => p.Password).WithMessage("Confirm password must match password");
        }
    }
}
