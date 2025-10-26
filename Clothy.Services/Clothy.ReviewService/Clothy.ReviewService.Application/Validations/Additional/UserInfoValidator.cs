using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.ValueObjects;
using FluentValidation;

namespace Clothy.ReviewService.Application.Validations.Additional
{
    public class UserInfoValidator : AbstractValidator<UserInfo>
    {
        public UserInfoValidator()
        {
            RuleFor(u => u.UserId)
                .NotEmpty().WithMessage("UserId is required.");

            RuleFor(u => u.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

            RuleFor(u => u.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

            RuleFor(x => x.PhotoUrl)
                .NotEmpty().WithMessage("PhotoUrl is required.")
                .Must(BeValidUrl).WithMessage("PhotoUrl must be a valid URL.")
                .Must(x => x.Contains("res.cloudinary")).WithMessage("PhotoUrl must be a Cloudinary URL.");
        }

        private bool BeValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var tmp)
                   && (tmp.Scheme == Uri.UriSchemeHttp || tmp.Scheme == Uri.UriSchemeHttps);
        }
    }
}
