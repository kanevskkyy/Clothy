using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.OrderItemDTOs;
using FluentValidation;

namespace Clothy.OrderService.BLL.FluentValidation.OrderItemValidation
{
    public class OrderItemCreateDTOValidator : AbstractValidator<OrderItemCreateDTO>
    {
        public OrderItemCreateDTOValidator()
        {
            RuleFor(x => x.ClotheId).NotEmpty().WithMessage("ClotheId is required.");

            RuleFor(x => x.ClotheName)
                .NotEmpty().WithMessage("ClotheName is required.")
                .MaximumLength(200).WithMessage("ClotheName cannot exceed 200 characters.");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be >= 0.");

            RuleFor(x => x.MainPhoto)
                .NotEmpty().WithMessage("MainPhoto is required.")
                .Must(BeValidUrl).WithMessage("MainPhoto must be a valid URL.")
                .Must(x => x.Contains("res.cloudinary")).WithMessage("MainPhoto must be a Cloudinary URL.");

            RuleFor(x => x.ColorId).NotEmpty().WithMessage("ColorId is required.");

            RuleFor(x => x.HexCode)
                .NotEmpty().WithMessage("HexCode is required.")
                .Matches("^#[0-9A-Fa-f]{6}$").WithMessage("HexCode must be valid hex.");

            RuleFor(x => x.SizeId).NotEmpty().WithMessage("SizeId is required.");

            RuleFor(x => x.SizeName)
                .NotEmpty().WithMessage("SizeName is required.")
                .MaximumLength(50).WithMessage("SizeName cannot exceed 50 characters.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        }

        private bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var tmp)
                   && (tmp.Scheme == Uri.UriSchemeHttp || tmp.Scheme == Uri.UriSchemeHttps);
        }
    }
}