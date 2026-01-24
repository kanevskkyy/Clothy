using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using FluentValidation;

namespace Clothy.CatalogService.BLL.FluentValidation.ClotheValidation
{
    public class ClotheUpdateDTOValidator : AbstractValidator<ClotheUpdateDTO>
    {
        public ClotheUpdateDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Slug is required.")
                .MaximumLength(60).WithMessage("Slug cannot exceed 60 characters.")
                .Must(IsLowercase).WithMessage("Slug must be lowercase.")
                .Matches(@"^[a-z0-9]+(-[a-z0-9]+)*$").WithMessage("Slug can contain only letters, numbers, and single dashes, cannot start or end with a dash, or contain consecutive dashes.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(x => x.Gender)
                .NotEmpty().WithMessage("Gender is required.");

            RuleFor(x => x.BrandId)
                .NotEmpty().WithMessage("BrandId is required.");

            RuleFor(x => x.ClothingTypeId)
                .NotEmpty().WithMessage("ClothingTypeId is required.");

            RuleFor(x => x.CollectionId)
                .NotEmpty().WithMessage("CollectionId is required.");
        }

        private bool IsLowercase(string slug)
        {
            return slug == slug.ToLowerInvariant();
        }
    }
}