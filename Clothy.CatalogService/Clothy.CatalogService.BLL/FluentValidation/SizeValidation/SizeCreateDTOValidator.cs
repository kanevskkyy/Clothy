using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.SizeDTOs;
using FluentValidation;

namespace Clothy.CatalogService.BLL.FluentValidation.SizeValidation
{
    public class SizeDTOValidator
    {
        public class SizeCreateDTOValidator : AbstractValidator<SizeCreateDTO>
        {
            public SizeCreateDTOValidator()
            {
                RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Size name is required.")
                    .MaximumLength(10).WithMessage("Size name must be at most 10 characters.");
            }
        }
    }
}
