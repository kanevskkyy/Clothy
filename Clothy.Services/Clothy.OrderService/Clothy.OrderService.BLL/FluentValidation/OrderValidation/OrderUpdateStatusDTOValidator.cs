using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.OrderDTOs;
using FluentValidation;

namespace Clothy.OrderService.BLL.FluentValidation.OrderValidation
{
    public class OrderUpdateStatusDTOValidator : AbstractValidator<OrderUpdateStatusDTO>
    {
        public OrderUpdateStatusDTOValidator()
        {
            RuleFor(x => x.StatusId).NotEmpty().WithMessage("StatusId is required.");
        }
    }
}
