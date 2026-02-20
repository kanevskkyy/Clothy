using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.BrandDTOs;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Clothy.CatalogService.BLL.FluentValidation.BrandValidation
{
    public class BrandCreateDTOValidator : AbstractValidator<BrandCreateDTO>
    {
        private readonly string[] permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".svg", ".webp" };

        public BrandCreateDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Slug is required.")
                .MaximumLength(100).WithMessage("Slug cannot exceed 100 characters.")
                .Must(IsLowercase).WithMessage("Slug must be lowercase.")
                .Matches(@"^[a-z0-9]+(-[a-z0-9]+)*$").WithMessage("Slug can contain only letters, numbers, and single dashes, cannot start or end with a dash, or contain consecutive dashes.");

            RuleFor(x => x.Photo)
                .NotNull().WithMessage("Photo is required.")
                .Must(HavePermittedExtension).WithMessage($"File must be one of: {string.Join(", ", permittedExtensions)}")
                .Must(HaveValidSize).WithMessage("File must be smaller than 5 MB.");
        }

        private bool HavePermittedExtension(IFormFile file)
        {
            if (file == null) return true; 
            string? extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            return extension != null && permittedExtensions.Contains(extension);
        }

        private bool IsLowercase(string slug)
        {
            return slug == slug.ToLowerInvariant();
        }

        private bool HaveValidSize(IFormFile file)
        {
            if (file == null) return true; 
            return file.Length > 0 && file.Length <= 5 * 1024 * 1024;
        }
    }
}
