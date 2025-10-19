using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using Clothy.ReviewService.Domain.ValueObjects;
using MediatR;

namespace Clothy.ReviewService.Application.Features.Reviews.Commands.CreateReview
{
    public record CreateReviewCommand(
        Guid ClotheItemId,
        UserInfo User,
        RatingValue Rating,
        TextValue Comment
    ) : ICommand<Guid>;
}
