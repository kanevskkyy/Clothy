using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.UserService.BLL.DTOs.UserDTOs;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Clothy.UserService.BLL.Validation.UserValidation
{
    public class UserUpdateDTOValidation : AbstractValidator<UserUpdateDTO>
    {
        private string[] permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".svg" };

        public UserUpdateDTOValidation()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First name is required.")
                .MaximumLength(50)
                .WithMessage("First name should be less than 50 symbols");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50)
                .WithMessage("Last name should be less than 50 symbols");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^(\+380|0)\d{9}$")
                .WithMessage("Phone number must be a valid Ukrainian number");

            RuleFor(x => x.Photo)
                .NotNull().WithMessage("Photo is required.")
                .Must(HavePermittedExtension).WithMessage($"File must be one of: {string.Join(", ", permittedExtensions)}")
                .Must(HaveValidSize).WithMessage("File must be smaller than 5 MB.");
        }

        private bool HavePermittedExtension(IFormFile file)
        {
            if (file == null) return true; 
            string? extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            return extension != null && permittedExtensions.Contains(extension);
        }

        private bool HaveValidSize(IFormFile file)
        {
            if (file == null) return true; 
            return file.Length > 0 && file.Length <= 5 * 1024 * 1024;
        }
    }
}
