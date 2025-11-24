using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Features.Reviews.Commands.CreateReview;
using FluentValidation;

namespace Clothy.ReviewService.Application.Validations.Reviews
{
    public class CreateReviewDTOValidator : AbstractValidator<CreateReviewDTO>
    {
        public CreateReviewDTOValidator()
        {
            RuleFor(x => x.ClotheItemId)
                .NotEmpty().WithMessage("ClotheItemId is required");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

            RuleFor(x => x.Comment)
                .NotEmpty().WithMessage("Comment is required")
                .MaximumLength(500).WithMessage("Comment must not exceed 500 characters");
        }
    }
}
