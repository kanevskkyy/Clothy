using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.SettlementDTOs;
using FluentValidation;

namespace Clothy.OrderService.BLL.FluentValidation.SettlementValidation
{
    public class SettlementUpdateDTOValidator : AbstractValidator<SettlementUpdateDTO>
    {
        public SettlementUpdateDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Settlement name is required.")
                .MaximumLength(100).WithMessage("Settlement name must not exceed 100 characters.");

            RuleFor(x => x.RegionId)
                .NotEmpty().WithMessage("RegionId is required.");
        }
    }
}
