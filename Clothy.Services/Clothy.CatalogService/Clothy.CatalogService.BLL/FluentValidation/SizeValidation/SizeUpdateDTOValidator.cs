using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.SizeDTOs;
using FluentValidation;

namespace Clothy.CatalogService.BLL.FluentValidation.SizeValidation
{
    public class SizeUpdateDTOValidator : AbstractValidator<SizeUpdateDTO>
    {
        public SizeUpdateDTOValidator()
        {
            RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Size name is required.")
                    .MaximumLength(10).WithMessage("Size name must be at most 10 characters.");

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Size slug is required.")
                .MaximumLength(10).WithMessage("Size slug must be at most 10 characters.")
                .Must(IsLowercase).WithMessage("Size slug must be lowercase.")
                .Matches(@"^[a-z0-9]+(-[a-z0-9]+)*$").WithMessage("Size slug can contain only letters, numbers, and single dashes, cannot start or end with a dash, or contain consecutive dashes.");
        }

        private bool IsLowercase(string slug)
        {
            return slug == slug.ToLowerInvariant();
        }
    }
}
