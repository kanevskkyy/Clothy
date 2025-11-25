using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.PickupPointsDTOs;
using FluentValidation;

namespace Clothy.OrderService.BLL.FluentValidation.PickupPointValidaton
{
    public class PickupPointUpdateDTOValidator : AbstractValidator<PickupPointUpdateDTO>
    {
        public PickupPointUpdateDTOValidator()
        {
            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required.")
                .MaximumLength(100).WithMessage("Address must be at most 100 characters.");

            RuleFor(x => x.DeliveryProviderId)
                .NotEmpty().WithMessage("DeliveryProviderId is required.");

            RuleFor(x => x.SettlementId)
                .NotEmpty().WithMessage("SettlementId is required.");
        }
    }
}
