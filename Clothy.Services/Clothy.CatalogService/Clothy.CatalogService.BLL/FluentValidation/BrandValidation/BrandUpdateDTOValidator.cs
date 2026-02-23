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
    public class BrandUpdateDTOValidator : AbstractValidator<BrandUpdateDTO>
    {
        private readonly string[] permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".svg", ".webp" };
        public BrandUpdateDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Slug is required.")
                .MaximumLength(100).WithMessage("Slug cannot exceed 100 characters.")
                .Must(IsLowercase).WithMessage("Slug must be lowercase.")
                .Matches(@"^[a-z0-9]+(-[a-z0-9]+)*$").WithMessage("Slug can contain only letters, numbers, and single dashes, cannot start or end with a dash, or contain consecutive dashes.");
        }
        
        private bool IsLowercase(string slug)
        {
            return slug == slug.ToLowerInvariant();
        }
    }
}
