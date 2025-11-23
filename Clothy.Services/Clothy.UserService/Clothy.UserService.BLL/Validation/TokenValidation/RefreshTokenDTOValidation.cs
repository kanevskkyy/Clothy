using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.UserService.BLL.DTOs.TokenDTOs;
using FluentValidation;

namespace Clothy.UserService.BLL.Validation.TokenValidation
{
    public class RefreshTokenDTOValidation : AbstractValidator<RefreshTokenDTO>
    {
        public RefreshTokenDTOValidation()
        {
            RuleFor(p => p.Token)
                .NotEmpty().WithMessage("Refresh token is required!");
        }
    }
}
