using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.OrderService.BLL.DTOs.OrderStatusDTOs;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Clothy.OrderService.BLL.FluentValidation.OrderStatusValidation
{
    public class OrderStatusCreateDTOValidator : AbstractValidator<OrderStatusCreateDTO>
    {
        private readonly string[] permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".svg" };

        public OrderStatusCreateDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.Icon)
                .NotNull().WithMessage("Icon is required.")
                .Must(HavePermittedExtension).WithMessage($"File must be one of: {string.Join(", ", permittedExtensions)}")
                .Must(HaveValidSize).WithMessage("File must be smaller than 5 MB.");
        }

        private bool HavePermittedExtension(IFormFile file)
        {
            if (file == null) return false;
            string? extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            return extension != null && permittedExtensions.Contains(extension);
        }

        private bool HaveValidSize(IFormFile file)
        {
            if (file == null) return false;
            return file.Length > 0 && file.Length <= 5 * 1024 * 1024;
        }
    }
}
