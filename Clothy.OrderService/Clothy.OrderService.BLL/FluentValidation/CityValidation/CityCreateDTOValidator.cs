using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.CityDTOs;
using FluentValidation;

namespace Clothy.OrderService.BLL.FluentValidation.CityValidation
{
    public class CityCreateDTOValidator : AbstractValidator<CityCreateDTO>
    {
        public CityCreateDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");
        }
    }
}
