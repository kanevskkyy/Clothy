using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Domain.Entities.QueryParameters;
using FluentValidation;

namespace Clothy.ReviewService.Application.Validations.QueryParams
{
    public class ReviewQueryParametersValidator : AbstractValidator<ReviewQueryParameters>
    {
        public ReviewQueryParametersValidator()
        {
            RuleFor(x => x.Rating)
                .InclusiveBetween(0, 5)
                .When(x => x.Rating.HasValue)
                .WithMessage("Rating must be between 0 and 5.");
        }
    }
}
