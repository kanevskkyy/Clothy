using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.CatalogService.BLL.DTOs.ColorDTOs;
using FluentValidation;

namespace Clothy.CatalogService.BLL.FluentValidation.ColorValidation
{
    public class ColorUpdateDTOValidator : AbstractValidator<ColorUpdateDTO>
    {
        public ColorUpdateDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Color name is required.")
                .MaximumLength(20).WithMessage("Color name must be at most 20 characters.");

            RuleFor(x => x.HexCode)
                .NotEmpty().WithMessage("Hex code is required.")
                .MaximumLength(7).WithMessage("Hex code must be at most 7 characters.")
                .Matches("^#[0-9A-Fa-f]{6}$").WithMessage("Hex code must be a valid color code (e.g. #A1B2C3).");
        }
    }
}
