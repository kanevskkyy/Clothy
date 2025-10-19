using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces.Services;

namespace Clothy.ReviewService.Application.Features.Reviews.Commands.CreateReview
{
    public class CreateReviewCommandHandler : ICommandHandler<CreateReviewCommand, Guid>
    {
        private IReviewService reviewService;

        public CreateReviewCommandHandler(IReviewService reviewService)
        {
            this.reviewService = reviewService;
        }

        public async Task<Guid> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
        {
            Review? review = await reviewService.AddReviewAsync(
                new Review(request.ClotheItemId, request.User, request.Rating, request.Comment),
                cancellationToken
            );
            return Guid.Parse(review.Id);
        }
    }
}
