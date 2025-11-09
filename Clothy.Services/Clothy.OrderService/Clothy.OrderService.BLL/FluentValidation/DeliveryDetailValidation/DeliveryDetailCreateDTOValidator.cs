using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.DeliveryDetailDTOs;
using FluentValidation;

namespace Clothy.OrderService.BLL.FluentValidation.DeliveryDetailValidation
{
    public class DeliveryDetailCreateDTOValidator : AbstractValidator<DeliveryDetailCreateDTO>
    {
        public DeliveryDetailCreateDTOValidator()
        {
            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("PhoneNumber is required")
                .MaximumLength(20).WithMessage("PhoneNumber cannot exceed 20 characters");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("FirstName is required")
                .MaximumLength(100).WithMessage("FirstName cannot exceed 100 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("LastName is required")
                .MaximumLength(100).WithMessage("LastName cannot exceed 100 characters");

            RuleFor(x => x.MiddleName)
                .NotEmpty().WithMessage("MiddleName is required")
                .MaximumLength(100).WithMessage("MiddleName cannot exceed 100 characters");

            RuleFor(x => x.PickupPointId)
                .NotEmpty().WithMessage("PickupPointId is required");
        }
    }
}
