using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.OrderDTOs;
using Clothy.OrderService.BLL.FluentValidation.DeliveryDetailValidation;
using Clothy.OrderService.BLL.FluentValidation.OrderItemValidation;
using FluentValidation;

namespace Clothy.OrderService.BLL.FluentValidation.OrderValidation
{
    public class OrderCreateDTOValidator : AbstractValidator<OrderCreateDTO>
    {
        public OrderCreateDTOValidator()
        {
            RuleFor(x => x.UserFirstName)
                .NotEmpty().WithMessage("UserFirstName is required.")
                .MaximumLength(100).WithMessage("UserFirstName cannot exceed 100 characters.");

            RuleFor(x => x.UserLastName)
                .NotEmpty().WithMessage("UserLastName is required.")
                .MaximumLength(100).WithMessage("UserLastName cannot exceed 100 characters.");

            RuleFor(x => x.Items).NotEmpty().WithMessage("Order must have at least one item.");
            
            RuleForEach(x => x.Items).SetValidator(new OrderItemCreateDTOValidator());

            RuleFor(x => x.DeliveryDetail).NotNull().WithMessage("DeliveryDetail is required.")
                                          .SetValidator(new DeliveryDetailCreateDTOValidator());
        }
    }
}
