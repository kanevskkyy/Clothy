using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.AuthService.BLL.DTOs.Users;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Clothy.AuthService.BLL.FluentValidation.Users
{
    public class UserUpdateDTOValidator : AbstractValidator<UserUpdateDTO>
    {
        private string[] permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".svg" };

        public UserUpdateDTOValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^\+380\d{9}$").WithMessage("Phone number must be in Ukrainian format: +380XXXXXXXXX.");

            RuleFor(x => x.Photo)
                .Must(HavePermittedExtension).WithMessage("Invalid file type.")
                .Must(HaveValidSize).WithMessage("File must be smaller than 5 MB.");
        }

        private bool HavePermittedExtension(IFormFile? file)
        {
            if (file == null) return true;
            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? "";
            return permittedExtensions.Contains(ext);
        }

        private bool HaveValidSize(IFormFile? file)
        {
            if (file == null) return true;
            return file.Length > 0 && file.Length <= 5 * 1024 * 1024;
        }
    }
}
