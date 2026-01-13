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
    public class ClotheUpdateDTOValidator : AbstractValidator<ClotheUpdateDTO>
    {
        public ClotheUpdateDTOValidator()
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

            RuleFor(x => x.BrandId)
                .NotEmpty().WithMessage("BrandId is required.");
            
            RuleFor(x => x.ClothingTypeId)
                .NotEmpty().WithMessage("ClothingTypeId is required.");
            
            RuleFor(x => x.CollectionId)
                .NotEmpty().WithMessage("CollectionId is required.");
        }
    }
}
