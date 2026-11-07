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

            When(p => p.Device != null, () =>
            {
                RuleFor(p => p.Device!.DeviceType).MaximumLength(20);
                RuleFor(p => p.Device!.DeviceInfo).MaximumLength(100);
                RuleFor(p => p.Device!.Os).MaximumLength(100);
                RuleFor(p => p.Device!.Browser).MaximumLength(100);
                RuleFor(p => p.Device!.Screen).MaximumLength(20);
            });
        }
    }
}
