using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.OrderStatusDTOs;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Clothy.OrderService.BLL.FluentValidation.OrderStatusValidation
{

    public class OrderStatusUpdateDTOValidator : AbstractValidator<OrderStatusUpdateDTO>
    {
        public OrderStatusUpdateDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
        }
    }
}
