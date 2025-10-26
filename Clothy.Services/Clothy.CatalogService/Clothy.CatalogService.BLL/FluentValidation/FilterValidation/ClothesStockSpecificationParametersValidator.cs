using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.Domain.QueryParameters;
using FluentValidation;

namespace Clothy.CatalogService.BLL.FluentValidation.FilterValidation
{
    public class ClothesStockSpecificationParametersValidator : AbstractValidator<ClothesStockSpecificationParameters>
    {
        public ClothesStockSpecificationParametersValidator()
        {
            RuleFor(x => x.MinQuantity)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MinQuantity.HasValue)
                .WithMessage("Minimum quantity cannot be negative");

            RuleFor(x => x.MaxQuantity)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MaxQuantity.HasValue)
                .WithMessage("Maximum quantity cannot be negative");

            RuleFor(x => x)
                .Must(x => !x.MinQuantity.HasValue || !x.MaxQuantity.HasValue || x.MaxQuantity >= x.MinQuantity)
                .WithMessage("Maximum quantity cannot be less than minimum quantity");
        }
    }
}
