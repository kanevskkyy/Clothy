using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.BasketService.BLL.DTOs;
using FluentValidation;

namespace Clothy.BasketService.BLL.Validation
{
    public class BasketItemUpdateDTOValidator : AbstractValidator<BasketItemUpdateDTO>
    {
        public BasketItemUpdateDTOValidator()
        {
            RuleFor(x => x.ClotheId)
                .NotEmpty().WithMessage("ClotheId is required");

            RuleFor(x => x.SizeId)
                .NotEmpty().WithMessage("SizeId is required");

            RuleFor(x => x.ColorId)
                .NotEmpty().WithMessage("ColorId is required");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");
        }
    }
}
