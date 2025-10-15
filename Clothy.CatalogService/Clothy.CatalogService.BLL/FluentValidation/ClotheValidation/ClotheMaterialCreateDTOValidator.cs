using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using FluentValidation;

namespace Clothy.CatalogService.BLL.FluentValidation.ClotheValidation
{
    public class ClotheMaterialCreateDTOValidator : AbstractValidator<ClotheMaterialCreateDTO>
    {
        public ClotheMaterialCreateDTOValidator()
        {
            RuleFor(x => x.MaterialId)
                .NotEmpty().WithMessage("MaterialId is required.");

            RuleFor(x => x.Percentage)
                .InclusiveBetween(0, 100)
                .WithMessage("Percentage must be between 0 and 100.");
        }
    }
}
