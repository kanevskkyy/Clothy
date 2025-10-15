using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.ClothingTypeDTOs;
using FluentValidation;

namespace Clothy.CatalogService.BLL.FluentValidation.ClothingTypeValidation
{
    public class ClothingTypeCreateDTOValidator : AbstractValidator<ClothingTypeCreateDTO>
    {
        public ClothingTypeCreateDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(50).WithMessage("Name must be at most 50 characters.");

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Slug is required.")
                .MaximumLength(100).WithMessage("Slug must be at most 100 characters.");
        }
    }
}
