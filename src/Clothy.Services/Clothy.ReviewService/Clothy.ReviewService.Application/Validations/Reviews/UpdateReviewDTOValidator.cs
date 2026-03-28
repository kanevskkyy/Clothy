using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Features.Reviews.Commands.UpdateReview;
using FluentValidation;

namespace Clothy.ReviewService.Application.Validations.Reviews
{
    public class UpdateReviewDTOValidator : AbstractValidator<UpdateReviewDTO>
    {
        public UpdateReviewDTOValidator()
        {
            RuleFor(x => x.Comment)
                .NotEmpty().WithMessage("Comment is required")
                .MaximumLength(500).WithMessage("Comment must not exceed 500 characters");

            RuleFor(x => x.Rating)
                .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");
        }
    }
}
