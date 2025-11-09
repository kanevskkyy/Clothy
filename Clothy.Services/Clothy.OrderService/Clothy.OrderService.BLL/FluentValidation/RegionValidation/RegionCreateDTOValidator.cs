using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.RegionDTOs;
using FluentValidation;

namespace Clothy.OrderService.BLL.FluentValidation.RegionValidation
{
    public class RegionCreateDTOValidator : AbstractValidator<RegionCreateDTO>
    {
        public RegionCreateDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Region name is required.")
                .MaximumLength(100).WithMessage("Region name must not exceed 100 characters.");

            RuleFor(x => x.CityId)
                .NotEmpty().WithMessage("CityId is required.");
        }
    }
}
