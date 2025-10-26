using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.Domain.QueryParameters;
using FluentValidation;

namespace Clothy.CatalogService.BLL.FluentValidation.FilterValidation
{
    public class ClotheItemSpecificationParametersValidator : AbstractValidator<ClotheItemSpecificationParameters>
    {
        public ClotheItemSpecificationParametersValidator()
        {
            RuleFor(x => x.MinPrice)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MinPrice.HasValue)
                .WithMessage("Minimum price cannot be negative!");

            RuleFor(x => x.MaxPrice)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MaxPrice.HasValue)
                .WithMessage("Maximum price cannot be negative!");

            RuleFor(x => x)
                .Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MaxPrice >= x.MinPrice)
                .WithMessage("Maximum price cannot be less than minimum price!");
        }
    }
}
