using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.CollectionDTOs;
using FluentValidation;

namespace Clothy.CatalogService.BLL.FluentValidation.CollectionValidation
{
    public class CollectionUpdateDTOValidator : AbstractValidator<CollectionUpdateDTO>
    {
        public CollectionUpdateDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must be at most 100 characters.");

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Slug is required.")
                .MaximumLength(60).WithMessage("Slug must be at most 60 characters.");
        }
    }

}
