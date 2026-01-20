using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.MaterialDTOs;
using FluentValidation;

namespace Clothy.CatalogService.BLL.FluentValidation.MaterialValidation
{
    public class MaterialUpdateDTOValidator : AbstractValidator<MaterialUpdateDTO>
    {
        public MaterialUpdateDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Material name is required.")
                .MaximumLength(50).WithMessage("Material name must be at most 50 characters.");

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Material slug is required.")
                .MaximumLength(60).WithMessage("Material slug must be at most 60 characters.")
                .Must(IsLowercase).WithMessage("Material slug must be lowercase.")
                .Matches(@"^[a-z0-9]+(-[a-z0-9]+)*$").WithMessage("Material slug can contain only letters, numbers, and single dashes, cannot start or end with a dash, or contain consecutive dashes.");
        }

        private bool IsLowercase(string slug)
        {
            return slug == slug.ToLowerInvariant();
        }
    }
}
