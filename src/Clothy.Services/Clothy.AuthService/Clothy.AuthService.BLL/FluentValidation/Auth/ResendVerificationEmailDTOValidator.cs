using Clothy.AuthService.BLL.DTOs.Auth;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.AuthService.BLL.FluentValidation.Auth
{
    public class ResendVerificationEmailDTOValidator : AbstractValidator<ResendVerificationEmailDTO>
    {
        public ResendVerificationEmailDTOValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email should be valid!");
        }
    }
}