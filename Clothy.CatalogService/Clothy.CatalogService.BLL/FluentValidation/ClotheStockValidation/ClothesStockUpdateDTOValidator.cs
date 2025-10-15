using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.ClotheStocksDTOs;
using FluentValidation;

namespace Clothy.CatalogService.BLL.FluentValidation.ClotheStockValidation
{
    public class ClothesStockUpdateDTOValidator : AbstractValidator<ClothesStockUpdateDTO>
    {
        public ClothesStockUpdateDTOValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.");
        }
    }
}
