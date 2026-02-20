using Clothy.CatalogService.BLL.DTOs.PhotoDTOs;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clothy.CatalogService.BLL.FluentValidation.ClotheValidation
{
    public class ClothePhotoCreateDTOValidator : AbstractValidator<ClothePhotoCreateDTO>
    {
        private readonly string[] permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".svg", ".webp" };

        public ClothePhotoCreateDTOValidator()
        {
            RuleFor(x => x.Photo)
                .NotNull().WithMessage("Photo is required.")
                .Must(HavePermittedExtension)
                    .WithMessage($"Photo must be one of: {string.Join(", ", permittedExtensions)}")
                .Must(HaveValidSize)
                    .WithMessage("Photo must be smaller than 5 MB");

            RuleFor(x => x.ColorId)
                .NotEmpty().WithMessage("ColorId is required.");
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
