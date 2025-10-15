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
        private readonly string[] permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".svg" };

        public ClotheCreateDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100);

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Slug is required.")
                .MaximumLength(60);

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(1000);

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(x => x.BrandId).NotEmpty().WithMessage("BrandId is required.");
            RuleFor(x => x.ClothingTypeId).NotEmpty().WithMessage("ClothingTypeId is required.");
            RuleFor(x => x.CollectionId).NotEmpty().WithMessage("CollectionId is required.");

            RuleFor(x => x.MainPhoto)
                .NotNull().WithMessage("Main photo is required.")
                .Must(HavePermittedExtension).WithMessage($"Main photo must be one of: {string.Join(", ", permittedExtensions)}")
                .Must(HaveValidSize).WithMessage("Main photo must be smaller than 5 MB");

            RuleForEach(x => x.AdditionalPhotos)
                .Must(HavePermittedExtension).WithMessage($"Additional photo must be one of: {string.Join(", ", permittedExtensions)}")
                .Must(HaveValidSize).WithMessage("Additional photo must be smaller than 5 MB")
                .When(x => x.AdditionalPhotos != null && x.AdditionalPhotos.Any());

            RuleFor(x => x.TagIds)
                .NotEmpty().WithMessage("At least one tag must be selected.");

            RuleForEach(x => x.Materials).SetValidator(new ClotheMaterialCreateDTOValidator());
        }

        private bool HavePermittedExtension(IFormFile file)
        {
            if (file == null) return true;
            string? extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            return extension != null && permittedExtensions.Contains(extension);
        }

        private bool HaveValidSize(IFormFile file)
        {
            if (file == null) return true;
            return file.Length > 0 && file.Length <= 5 * 1024 * 1024;
        }
    }
}
