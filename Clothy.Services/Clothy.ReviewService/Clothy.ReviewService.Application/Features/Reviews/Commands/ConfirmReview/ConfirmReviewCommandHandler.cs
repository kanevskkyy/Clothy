using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clothy.ReviewService.Application.Interfaces.Commands;
using Clothy.ReviewService.Domain.Entities;
using Clothy.ReviewService.Domain.Interfaces;
using Clothy.Shared.Helpers.Exceptions;

namespace Clothy.ReviewService.Application.Features.Reviews.Commands.ConfirmReview
{
    public class ConfirmReviewCommandHandler : ICommandHandler<ConfirmReviewCommand, Review>
    {
        private IReviewRepository reviewRepository;

        public ConfirmReviewCommandHandler(IReviewRepository reviewRepository)
        {
            this.reviewRepository = reviewRepository;
        }

        public async Task<Review> Handle(ConfirmReviewCommand request, CancellationToken cancellationToken)
        {
            Review? review = await reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);
            if (review == null) throw new NotFoundException($"Review with ID {request.ReviewId} not found.");

            review.ConfirmStatus();

            await reviewRepository.UpdateAsync(review, cancellationToken);
            return review;
        }
    }
}
