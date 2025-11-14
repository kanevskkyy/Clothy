using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.OrderItemDTOs;
using FluentValidation;

namespace Clothy.OrderService.BLL.FluentValidation.OrderItemValidation
{
    public class OrderItemCreateDTOValidator : AbstractValidator<OrderItemCreateDTO>
    {
        public OrderItemCreateDTOValidator()
        {
            RuleFor(x => x.ClotheId).NotEmpty().WithMessage("ClotheId is required.");

            RuleFor(x => x.ColorId).NotEmpty().WithMessage("ColorId is required.");

            RuleFor(x => x.SizeId).NotEmpty().WithMessage("SizeId is required.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        }
    }
}