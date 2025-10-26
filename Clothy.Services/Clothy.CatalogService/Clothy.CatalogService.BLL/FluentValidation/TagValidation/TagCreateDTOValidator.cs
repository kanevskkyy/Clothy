using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.TagDTOs;
using FluentValidation;

namespace Clothy.CatalogService.BLL.FluentValidation.TagValidation
{
    public class TagCreateDTOValidator : AbstractValidator<TagCreateDTO>
    {
        public TagCreateDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tag name is required.")
                .MaximumLength(50).WithMessage("Tag name must be at most 50 characters.");
        }
    }
}
