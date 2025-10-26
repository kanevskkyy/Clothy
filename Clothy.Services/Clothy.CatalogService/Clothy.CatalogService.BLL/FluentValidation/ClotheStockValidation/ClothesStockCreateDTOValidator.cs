using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.ClotheStocksDTOs;
using FluentValidation;

namespace Clothy.CatalogService.BLL.FluentValidation.ClotheStockValidation
{
    public class ClothesStockCreateDTOValidator : AbstractValidator<ClothesStockCreateDTO>
    {
        public ClothesStockCreateDTOValidator()
        {
            RuleFor(x => x.ClotheId)
                .NotEmpty().WithMessage("ClotheId is required.");

            RuleFor(x => x.SizeId)
                .NotEmpty().WithMessage("SizeId is required.");

            RuleFor(x => x.ColorId)
                .NotEmpty().WithMessage("ColorId is required.");

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.");
        }
    }
}
