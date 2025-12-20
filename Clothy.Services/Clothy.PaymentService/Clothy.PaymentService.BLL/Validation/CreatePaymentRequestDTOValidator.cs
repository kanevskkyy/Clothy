using Clothy.PaymentService.BLL.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.PaymentService.BLL.Validation
{
    public class CreatePaymentRequestDTOValidator : AbstractValidator<CreatePaymentRequestDTO>
    {
        public CreatePaymentRequestDTOValidator()
        {
            RuleFor(p => p.OrderId)
                .NotEmpty().WithMessage("OrderID cannot be empty!");
        }
    }
}
