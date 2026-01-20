using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.ClotheDTOs;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Clothy.CatalogService.BLL.FluentValidation.ClotheValidation
{
    public class ClotheCreateDTOValidator : AbstractValidator<ClotheCreateDTO>
    {
        public ClotheCreateDTOValidator()
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

            RuleFor(x => x.BrandId)
                .NotEmpty().WithMessage("BrandId is required.");

            RuleFor(x => x.ClothingTypeId)
                .NotEmpty().WithMessage("ClothingTypeId is required.");

            RuleFor(x => x.CollectionId)
                .NotEmpty().WithMessage("CollectionId is required.");

            RuleFor(x => x.AdditionalPhotos)
                .NotEmpty().WithMessage("At least one additional photo is required.");

            RuleForEach(x => x.AdditionalPhotos)
                .SetValidator(new ClothePhotoCreateDTOValidator())
                .When(x => x.AdditionalPhotos != null && x.AdditionalPhotos.Any())
                .WithMessage("All additional photos must be valid.");

            RuleFor(x => x.TagIds)
                .NotEmpty().WithMessage("At least one tag must be selected.");

            RuleForEach(x => x.Materials)
                .SetValidator(new ClotheMaterialCreateDTOValidator())
                .WithMessage("All materials must be valid.");
        }

        private bool IsLowercase(string slug)
        {
            return slug == slug.ToLowerInvariant();
        }
    }
}